#include "AudioPluginUtil.h"
#include <algorithm>
#include <iostream>
#include <iomanip>

namespace Plugin_Friction
{
	const float m_pi = 3.141592653589793f;

	const float Fs = 48000.0f;
	const float q_r = 500.0f;
	const float m_r = 0.001f;
	const float m_b = 0.05f;

	const float sig0 = 10000.0f;
	const float sig1 = 0.1f*sqrtf(sig0);
	const float sig2 = 0.4f;
	const float sig3 = 0.8f;
	const float v_s = 0.1f;
	const float mu_d = 0.2f;
	const float mu_s = 0.4f;

	const float errMax = 1e-13f;

	enum Param
	{
		P_TRIGGER,
		P_PICKUP,
		P_FREQ,
		P_NOISE,
		P_STRIBECK,
		P_FN,
		P_RESET,
		P_NUM
	};

	struct EffectData
	{
		struct Data
		{
			float p[P_NUM];

			// Resonator

			float b_r[2][3];
			float A_r[2][2][3];
			float xv_r[2][3];

			// Bow
			float b_b[2];
			float A_b[2][2];
			float xv_b[2];

			float f_s;
			float f_c;
			float Zba;
			float fe_b;

			float K1;
			float bv;
			float bv_r;
			float bv_b;
			float K2;

			float yPrev;
			float zPrev;
			float z_Ti;
			float f_tot_b;
			float f_tot_r;

			float h0;

			float fader;
			bool stateOn;
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
		float detTemp = 0.0f;

		data->p[P_FN] = 1.0f;
		data->p[P_STRIBECK] = 0.3f;

		std::cout << std::setprecision(14);

		float omega_r[3];
		float g_r[3];
		data->bv_r = 0.0f;

		float freq[3] = { data->p[P_FREQ], data->p[P_FREQ] * 2.0f, data->p[P_FREQ] * 3.0f };

		for (int i = 0; i < 3; i++)
		{
			omega_r[i] = 2.0f * m_pi*freq[i];
			g_r[i] = omega_r[i] / q_r;
			detTemp = (Fs*Fs + g_r[i] * Fs / 2.0f + (omega_r[i] * omega_r[i]) / 4.0f);

			data->A_r[0][0][i] = 1.0f / detTemp * (detTemp - (omega_r[i] * omega_r[i]) / 2.0f);
			data->A_r[1][0][i] = 1.0f / detTemp * (-Fs * omega_r[i] * omega_r[i]);
			data->A_r[0][1][i] = 1.0f / detTemp * Fs;
			data->A_r[1][1][i] = 1.0f / detTemp * (2.0f * Fs*Fs - detTemp);

			data->b_r[0][i] = (1.0f / m_r)*(1.0f / (4.0f * detTemp));
			data->b_r[1][i] = (1.0f / m_r)*(1.0f / (4.0f * detTemp)) * 2.0f * Fs;

			data->bv_r += data->b_r[1][i];
		}

		detTemp = Fs * Fs;
		data->A_b[0][0] = 1.0f / detTemp * (detTemp);
		data->A_b[1][0] = 1.0f / detTemp;
		data->A_b[0][1] = 1.0f / detTemp * Fs;
		data->A_b[1][1] = 1.0f / detTemp * (2.0f * Fs*Fs - detTemp);
		data->b_b[0] = (1.0f / m_b)*(1.0f / (4.0f * detTemp));
		data->b_b[1] = (1.0f / m_b)*(1.0f / (4.0f * detTemp)) * 2.0f * Fs;

		data->f_s = mu_s * data->p[P_FN];
		data->f_c = mu_d * data->p[P_FN];
		data->Zba = 0.7f * data->f_c / sig0;

		data->fe_b = data->f_c + (data->f_s - data->f_c)*expf(-(data->p[P_STRIBECK] / v_s)*(data->p[P_STRIBECK] / v_s)) + sig2 * data->p[P_STRIBECK];

		data->K2 = 1.0f / (2.0f * Fs);

		data->bv_b = data->b_b[1];
		data->bv = data->bv_b + data->bv_r;

		data->K1 = -data->bv / (1.0f + sig2 * data->bv)*(sig0 / (2.0f * Fs) + sig1);

		// Reset arrays and data
		if (!data->stateOn)
		{
			data->stateOn = true;
			data->h0 = 0.0f;
			data->zPrev = 0.0f;
			data->yPrev = 0.0f;
			data->f_tot_b = 0.0f;
			data->f_tot_r = 0.0f;

			for (int j = 0; j < 3; j++)
			{
				data->xv_r[0][j] = 0.0f;
				data->xv_r[1][j] = 0.0f;
			}
			data->xv_b[0] = 0.0f;
			data->xv_b[1] = 0.0f;
		}

		//std::cout << "fe_b = " << data->fe_b << "; K2 = " << data->K2 << "; K1 = " << data->K1 << ";\n";
	}

	static void Reset(EffectData::Data* data)
	{
		// Reset arrays and data
		data->stateOn = false;
	}

	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition[numparams];
		RegisterParameter(definition, "Trigger", "", -1.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_TRIGGER, "Trigger a signal");
		RegisterParameter(definition, "Pickup", "", 1.0f, 1000.0f, 1.0f, 1.0f, 1.0f, P_PICKUP, "Output is multiplied by this value");
		RegisterParameter(definition, "Noisiness", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_NOISE, "Noisness sigma3");
		RegisterParameter(definition, "Freq", "Hz", 100.0f, 250.0f, 140.0f, 1.0f, 1.0f, P_FREQ, "Fundamental Frequency");
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
			else
				Reset(data);
		}
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

	float sign(float f)
	{
		if (f > 0) return 1;
		if (f < 0) return -1;
		return 0;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		float wetTarget = ((state->flags & UnityAudioEffectStateFlags_IsPlaying) && !(state->flags & (UnityAudioEffectStateFlags_IsMuted | UnityAudioEffectStateFlags_IsPaused))) ? 1.0f : 0.0f;

		// Semi Cache
		float x = 0.0f;
		float v_Ti = 0.0f;
		float v_bSum = 0.0;
		float v_rSum = 0.0f;
		float tempX = 0.0f;

		float maxIt = 0.0f;

		for (unsigned int n = 0; n < length; n++)
		{
			// Resonators
			v_rSum = 0.0f;
			for (int j = 0; j < 3; j++)
			{
				tempX = data->A_r[0][0][j] * data->xv_r[0][j] + data->A_r[0][1][j] * data->xv_r[1][j] + data->b_r[0][j] * data->f_tot_r;
				data->xv_r[1][j] = data->A_r[1][0][j] * data->xv_r[0][j] + data->A_r[1][1][j] * data->xv_r[1][j] + data->b_r[1][j] * data->f_tot_r;
				data->xv_r[0][j] = tempX;


				v_rSum += data->xv_r[1][j];
			}

			// Bow
			tempX = data->A_b[0][0] * data->xv_b[0] + data->A_b[0][1] * data->xv_b[1] + data->b_b[0] * data->f_tot_b;
			data->xv_b[1] = data->A_b[1][0] * data->xv_b[0] + data->A_b[1][1] * data->xv_b[1] + data->b_b[1] * data->f_tot_b;
			data->xv_b[0] = tempX;

			v_bSum = data->xv_b[1];

			// Computable parts
			float z_Ti = data->zPrev + 1.0f / (2.0f * Fs)*data->yPrev;

			//float w = 0.0f;
			float w = data->random.GetFloat(-1.0f, 1.0f) *abs(v_bSum);

			v_Ti = 1.0f / (1.0f + sig2 * data->bv)*((v_bSum + data->bv_b * (data->fe_b - sig0 * z_Ti)) + (-v_rSum - data->bv_r * (sig0*z_Ti)));

			// NEWTON-RHAPSON HERE //
			float count = 1;
			float err = 99;
			while (err > errMax && count < 10 && wetTarget == 1.0f)
			{
				float vNew = v_Ti + data->K1 * data->h0;
				float zNew = z_Ti + data->K2 * data->h0;

				// Find Zss			
				float Zss = (sign(vNew) / sig0)*(data->f_c + (data->f_s - data->f_c)*expf(-(vNew / v_s)*(vNew / v_s)));

				float aNew = 0.0f;
				// Find alpha tilde
				if (sign(zNew) != sign(vNew))
					aNew = 0.0f;
				else if (fabs(zNew) < data->Zba)
					aNew = 0.0f;
				else if (fabs(zNew) > Zss)
					aNew = 1.0f;
				else
					aNew = 0.5f*(1.0f + sin(m_pi*((zNew - 0.5f*(Zss + data->Zba)) / (Zss - data->Zba))));


				// Compute g
				float gNom = vNew * (1.0f - aNew * zNew / Zss) - data->h0;

				// Compute derivatives needed for derivative of g
				float ZssvDeri = -sign(vNew) * (2.0f * vNew) / (sig0*v_s*v_s) * (data->f_s - data->f_c)*exp(-(vNew / v_s)*(vNew / v_s));

				float azDeri = 0.0f;
				float avDeri = 0.0f;

				if ((data->Zba < fabs(zNew)) && (fabs(zNew) < Zss) && (sign(vNew) == sign(zNew)))
				{
					float temp = 0.5f*m_pi * cos(m_pi*(zNew - 0.5f*(Zss + data->Zba)) / (Zss - data->Zba));
					azDeri = temp * (1.0f / (Zss - data->Zba));
					avDeri = temp * ((ZssvDeri*(data->Zba - zNew)) / ((Zss - data->Zba)*(Zss - data->Zba)));
				}

				float derZ = -(vNew / Zss)*(zNew*azDeri + aNew);
				float derV = 1.0f - zNew * (((aNew + vNew * avDeri)*Zss - aNew * vNew*ZssvDeri) / (Zss*Zss));
				float gDeri = derV * data->K1 + derZ * data->K2 - 1.0f;

				float h1 = data->h0 - gNom / gDeri;

				count = count + 1;
				maxIt = fmax(count, maxIt);

				err = fabs(h1 - data->h0);

				data->h0 = h1;
			}

			// NEWTON-RHAPSON OVER //

			float dotz = data->h0;
			float v = v_Ti + data->K1 * dotz;
			float z = z_Ti + data->K2 * dotz;
			data->zPrev = z;
			data->yPrev = dotz;

			float f_fr = sig0 * z + sig1 * dotz + sig2 * v + data->p[P_NOISE] * w;

			data->f_tot_b = data->fe_b - f_fr;
			data->f_tot_r = f_fr;

			x = 0.0f;
			for (int j = 0; j < 3; j++)
			{
				data->xv_r[0][j] = data->xv_r[0][j] + data->b_r[0][j] * data->f_tot_r;
				data->xv_r[1][j] = data->xv_r[1][j] + data->b_r[1][j] * data->f_tot_r;

				x += data->xv_r[0][j];
			}

			data->xv_b[0] = data->xv_b[0] + data->b_b[0] * data->f_tot_b;
			data->xv_b[1] = data->xv_b[1] + data->b_b[1] * data->f_tot_b;

			data->fader = data->p[P_TRIGGER] == 0.0f ? fmax(data->fader - 0.001f, 0.0f) : fmin(data->fader + 0.001f, 1.0f);

			// For each channel
			for (int i = 0; i < outchannels; i++)
			{
				outbuffer[n * outchannels + i] = data->p[P_PICKUP] * x * wetTarget * data->fader;
			}
		}
		return UNITY_AUDIODSP_OK;
	}
}
