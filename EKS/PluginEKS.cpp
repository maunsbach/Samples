#include "AudioPluginUtil.h"
#include <algorithm>;
#include <iostream>;

namespace Plugin_EKS
{
	const int MAXDEL = 1024;
	const float SAMPLERATE = 48000.0f;

	enum Param
	{
		P_FREQ,
		P_TRIGGER,
		P_PICKDIR_P,
		P_PICKPOS_BETA,
		P_DAMP_B,
		P_DAMP_SEC,
		P_DYNLEV_L,
		P_NUM
	};

	struct EffectData
	{
		struct Data
		{
			float p[P_NUM];
			//float s;
			//float fader;
			int N;
			int n;
			//bool flagNewData;
			float noiseBurst[MAXDEL];
			int noiseIdx;
			float outPickDir[MAXDEL];
			float outPickPos[MAXDEL];
			float outDel[MAXDEL];
			int combDel;
			float outDamp[MAXDEL];
			float outStringTuning[MAXDEL];
			float nStringTuning;
			float outDynLev[MAXDEL];

			float out[MAXDEL];
			Random random;
		};
		union
		{
			Data data;
			unsigned char pad[(sizeof(Data) + 15) & ~15]; // This entire structure must be a multiple of 16 bytes (and and instance 16 byte aligned) for PS3 SPU DMA requirements
		};
	};

	static void ReTrigger(EffectData::Data* data)
	{
		data->N = (int)floor(SAMPLERATE / data->p[P_FREQ] - 1.0f);
		data->nStringTuning = SAMPLERATE / data->p[P_FREQ] - data->N - 1.0f;
		int combDel = floor(data->p[P_PICKPOS_BETA] * data->N);

		for (int n = 0; n < data->N; n++)
		{
			data->noiseBurst[n] = data->random.GetFloat(-1.0f, 1.0f);
			data->outPickDir[n] = (1 - data->p[P_PICKDIR_P]) * data->noiseBurst[n] + data->p[P_PICKDIR_P] * data->outPickDir[(n-1) & 0x3FF];
			data->outPickPos[n] = data->outPickDir[n] - data->outPickDir[(n - combDel) & 0x3FF];
		}

		data->n = data->N;
		data->noiseIdx = 0;
	}

	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition[numparams];
		RegisterParameter(definition, "Trigger", "", -1.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_TRIGGER, "Trigger a signal");
		RegisterParameter(definition, "Frequency", "Hz", 60.0f, 3000.0f, 440.0f, 1.0f, 3.0f, P_FREQ, "Frequency of sine oscillator that is multiplied with the input signal");
		RegisterParameter(definition, "Pick Direction", "", 0.0f, 1.0f, 0.9f, 1.0f, 1.0f, P_PICKDIR_P, "Pick direction. 0 is up, 0.9 is down.");
		RegisterParameter(definition, "Pick Position", "", 0.0f, 1.0f, 0.13f, 1.0f, 1.0f, P_PICKPOS_BETA, "Ratio of pick position on fretboard. 0 is bridge, 1 is nut.");
		RegisterParameter(definition, "Damping", "", 0.00f, 1.0f, 1.0f, 1.0f, 1.0f, P_DAMP_B, "Brightness 0-1");
		RegisterParameter(definition, "Damping Time", "s", 0.5f, 14.0f, 14.0f, 1.0f, 1.0f, P_DAMP_SEC, "Time in seconds to get to -60db.");
		RegisterParameter(definition, "Dynamic Level", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_DYNLEV_L, "Dynamic level for more energetic pick.");
		return numparams;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		EffectData* effectdata = new EffectData;
		memset(effectdata, 0, sizeof(EffectData));
		state->effectdata = effectdata;
		InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->data.p);
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		delete data;
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		if (index >= P_NUM)
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		data->p[index] = value;

		// Reset on Trigger
		if (index == P_TRIGGER)
		{
			if (data->p[P_TRIGGER] != 0.0f)
				ReTrigger(data);
		}
		else if (index == P_FREQ)
			data->N = (int)floor(SAMPLERATE / data->p[P_FREQ]);
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		if (index >= P_NUM)
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		if (value != NULL)
			*value = data->p[index];
		if (valuestr != NULL)
			valuestr[0] = 0;
		return UNITY_AUDIODSP_OK;
	}

	int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback(UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
	{
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		float wetTarget = ((state->flags & UnityAudioEffectStateFlags_IsPlaying) && !(state->flags & (UnityAudioEffectStateFlags_IsMuted | UnityAudioEffectStateFlags_IsPaused))) ? 1.0f : 0.0f;

		float dampRho = pow(0.001f, (1.0f / (data->p[P_FREQ] * data->p[P_DAMP_SEC])));
		float damph0 = (1.0f + data->p[P_DAMP_B]) * 0.5f;
		float damph1 = (1.0f - data->p[P_DAMP_B]) * 0.25f;
		
		float w = (kPI*data->p[P_FREQ]) / SAMPLERATE;
		float enerC0 = w / (1.0f + w);
		float enerC1 = ((1.0f - w) / (1.0f + w));
		float L0 = pow(data->p[P_DYNLEV_L], (1.0f / 3.0f));

		//std::cout << "L0 " << L0 << "  enerC1 " << enerC1 << " enerC0 " << enerC0 << " w " << w << "\n";
		//std::cout << "First " << (data->p[P_DYNLEV_L] * L0) << " Second " << (1.0f - data->p[P_DYNLEV_L]) << "\n";

		for (unsigned int n = 0; n < length; n++)
		{
			// Only apply noise in the beginning
			float noise = 0.0f;
			if (data->noiseIdx < data->N)
			{
				noise = data->outPickPos[data->noiseIdx];
				data->noiseIdx++;
			}

			data->outDel[data->n] = noise + data->outStringTuning[(data->n - data->N) & 0x3FF];

			data->outDynLev[data->n] = enerC0 * (data->outDel[data->n] + data->outDel[(data->n-1) & 0x3FF]) + enerC1 * data->outDynLev[(data->n - 1) & 0x3FF];
			data->outDynLev[data->n] = data->p[P_DYNLEV_L] * L0*data->outDel[data->n] + (1.0f - data->p[P_DYNLEV_L])*data->outDynLev[data->n];

			data->outDamp[data->n] = dampRho * (damph1*(data->outDel[data->n] + data->outDel[(data->n - 2) & 0x3FF]) + damph0 * data->outDel[(data->n - 1) & 0x3FF]);
			data->outStringTuning[data->n] = (1.0f - data->nStringTuning)*data->outDamp[data->n] + data->nStringTuning*data->outDamp[(data->n - 1) & 0x3FF];
				
			for (int i = 0; i < outchannels; i++)
			{
				float x = data->outDynLev[data->n];

				outbuffer[n * outchannels + i] = x * wetTarget;
			}

			data->n = (data->n+1) & 0x3FF;
		}

		return UNITY_AUDIODSP_OK;
	}
}