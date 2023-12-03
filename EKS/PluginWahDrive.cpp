#include "AudioPluginUtil.h"
#include <algorithm>;

namespace Plugin_WahDrive
{
	const int MAXDEL = 1024;
	const float SAMPLERATE = 48000.0f;
	const float th = 1.0f / 3.0f;

	enum Param
	{
		P_DRIVE,
		P_WAHCENTER,
		P_WAHMIX,
		P_NUM
	};

	struct EffectData
	{
		struct Data
		{
			float p[P_NUM];
			float delY;
			float delLow;
		};
		union
		{
			Data data;
			unsigned char pad[(sizeof(Data) + 15) & ~15]; // This entire structure must be a multiple of 16 bytes (and and instance 16 byte aligned) for PS3 SPU DMA requirements
		};
	};


	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition[numparams];
		RegisterParameter(definition, "Drive", "Gain", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_DRIVE, "Overdrive Drive");
		RegisterParameter(definition, "WahCenter", "Hz", 500.0f, 3000.0f, 2000.0f, 1.0f, 1.0f, P_WAHCENTER, "Center frequency of the Wah Band Pass");
		RegisterParameter(definition, "WahMix", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_WAHMIX, "Wah Wah mix. Dry to Wet.");
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


	float Overdrive(float x)
	{
		float y = 0.0f;
		if (fabs(x) < th)
			return 2.0f * x;
		else if (fabs(x) > 2.0f * th)
		{
			if (x > 0)
				return 1.0f;
			if (x < 0)
				return -1.0f;
		}
		else
		{
			if (x > 0)
				return (3.0f - (2.0f - 3.0f*x)*(2.0f - 3.0f*x)) / 3.0f;

			else
				return -(3.0f - (2.0f - fabs(x) * 3.0f)*(2.0f - fabs(x) * 3.0f)) / 3.0f;
		}
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		float wetTarget = ((state->flags & UnityAudioEffectStateFlags_IsPlaying) && !(state->flags & (UnityAudioEffectStateFlags_IsMuted | UnityAudioEffectStateFlags_IsPaused))) ? 1.0f : 0.0f;

		float F1 = 2 * sin((kPI*data->p[P_WAHCENTER]) / SAMPLERATE);
		float preGain = pow(10, 2.0f*data->p[P_DRIVE]);

		float x = 0.0f;

		for (unsigned int n = 0; n < length; n++)
		{
			for (int i = 0; i < outchannels; i++)
			{
				float inX = inbuffer[n * outchannels + i];
				// Wah wah
				data->delY = F1 * (inX - data->delLow - 0.4*data->delY) + data->delY;

				// Wet/Dry Mix
				x = data->p[P_WAHMIX] * data->delY + (1.0f - data->p[P_WAHMIX])*inX;
				
				// Save delayed low pass value
				data->delLow = F1 * data->delY + data->delLow;

				x = x*preGain;
				x = Overdrive(x);
			

				outbuffer[n * outchannels + i] = x*wetTarget;
			}
		}

		return UNITY_AUDIODSP_OK;
	}
}
