using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SProc : MonoBehaviour
{
    // Band Pass Filter (constant 0 dB peak gain)
    public float[] getBPF(float f, float Q, float sr)
    {
        
        float fc = f / sr;
        Q = Q / fc;
        float omega = 2 * Mathf.PI * fc;
        float alpha = Mathf.Sin(omega) / (2 * Q);

        float b0 = alpha;
        float b1 = 0;
        float b2 = -alpha;
        float a0 = 1 + alpha;
        float a1 = -2 * Mathf.Cos(omega);
        float a2 = 1 - alpha;
        float[] coefficients = new float[] { b0, b1, b2, a0, a1, a2};

        return coefficients;
    }

    public float[][] getBPFMatrix(float[] f, float[] Q, float sr)
    {
        int modes = f.Length;
        float[][] bps = new float[modes][];

        for (int i = 0; i < modes; i++)
        {
            bps[i] = getBPF(f[i], Q[i], sr);

        }

        return bps;
    }

    // Band Pass Filter (constant skirt gain, peak gain = Q)
    public float[] getBPF2(float f, float Q, float sr)
    {
        float fc = f / sr;
        Q = Q / fc;
        float omega = 2 * Mathf.PI * fc;
        float alpha = Mathf.Sin(omega) / (2 * Q);

        float b0 = Q * alpha;
        float b1 = 0;
        float b2 = -Q * alpha;
        float a0 = 1 + alpha;
        float a1 = -2 * Mathf.Cos(omega);
        float a2 = 1 - alpha;
        float[] coefficients = new float[] { b0, b1, b2, a0, a1, a2 };

        return coefficients;
    }

    // Low Pass Filter
    public float[] getLPF(float f, float Q, float sr)
    {
        float fc = f / sr;
        //Q = Q / fc;
        float omega = 2 * Mathf.PI * fc;
        float alpha = Mathf.Sin(omega) / (2 * Q);

        float b0 = 0.5f * (1 - Mathf.Cos(omega));
        float b1 = 1 - Mathf.Cos(omega);
        float b2 = 0.5f * (1 - Mathf.Cos(omega));
        float a0 = 1 + alpha;
        float a1 = -2 * Mathf.Cos(omega);
        float a2 = 1 - alpha;
        float[] coefficients = new float[] { b0, b1, b2, a0, a1, a2 };

        return coefficients;
    }

    // High Pass Filter
    public float[] getHPF(float f, float Q, float sr)
    {
        float fc = f / sr;
        //Q = Q / fc;
        float omega = 2 * Mathf.PI * fc;
        float alpha = Mathf.Sin(omega) / (2 * Q);

        float b0 = 0.5f * (1 + Mathf.Cos(omega));
        float b1 = -(1 + Mathf.Cos(omega));
        float b2 = 0.5f * (1 + Mathf.Cos(omega));
        float a0 = 1 + alpha;
        float a1 = -2 * Mathf.Cos(omega);
        float a2 = 1 - alpha;
        float[] coefficients = new float[] { b0, b1, b2, a0, a1, a2 };

        return coefficients;
    }

    // Direct form with input (and delayed input) multiplyer, gamma.
    public float directForm(float x0, float x1, float x2, float[] coef, float y1, float y2, float gamma)
    {
        //Debug.Log(""+ coef[0] / coef[3]+ "; " + coef[1] / coef[3] + "; " + coef[2] / coef[3] + "; " + coef[4] / coef[3] + "; " + coef[5] / coef[3]);

        return gamma * ((coef[0]/coef[3]) * x0 + (coef[1]/coef[3]) * x1 + (coef[2]/coef[3]) * x2) 
                            - (coef[4]/coef[3]) * y1 - (coef[5]/coef[3]) * y2;
    }

    public float directForm(float x0, float x1, float x2, float[] coef, float y1, float y2)
    {
        //Debug.Log(""+ coef[0] / coef[3]+ "; " + coef[1] / coef[3] + "; " + coef[2] / coef[3] + "; " + coef[4] / coef[3] + "; " + coef[5] / coef[3]);

        return (coef[0] / coef[3]) * x0 + (coef[1] / coef[3]) * x1 + (coef[2] / coef[3]) * x2
                            - (coef[4] / coef[3]) * y1 - (coef[5] / coef[3]) * y2;
    }

    // Returns damping matrix (not for real-time)
    public float[,] getDampings(float[] d, int len, float sr)
    {
        float b0 = d[0];
        float b1 = d[1];
        float a0 = d[2];

        float[,] dampings = new float[3,len];

        b0 = b0 / sr;
        for (int i = 0; i < len; i++)
        {
            dampings[0,i] = Mathf.Exp(-b0 * i);
        }

        b1 = b1 / sr;
        for (int i = 0; i < len; i++)
        {
            dampings[1,i] = Mathf.Exp(-b1 * i);
        }

        a0 = a0 / sr;
        for (int i = 0; i < len; i++)
        {
            dampings[2,i] = Mathf.Exp(-a0 * i) - 1;
        }

        return dampings;
    }

    // Return damping vector [b0 b1 a1] values for real-time implementation.
    public float[] getDampings2(float[] d, int i, float sr)
    {
        float[] dampings = new float[3];
        dampings[0] = Mathf.Exp(-(d[0]/sr) * i);
        dampings[1] = Mathf.Exp(-(d[1] / sr) * i);
        dampings[2] = Mathf.Exp(-(d[2] / sr) * i) - 1;

        return dampings;
    }

    // Not used
    public float release(int i, int divisions)
    {
        return 1f/divisions * i;
    }
}