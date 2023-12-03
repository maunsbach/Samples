using UnityEngine;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class EmitterBanded : MonoBehaviour
{
    private SProc sProc;

    private System.Random RandomNumber = new System.Random();
    private bool isRunning = true;
    private float normalizeAmplifier = 0.1f;
    private int index;
    private int decayTime = 48000;
    private float impact;

    public string materialName = "";

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isRunning)
            return;

        // The length of the data = 1024
        int dataLen = data.Length / channels;

        // Counter for for each clip of data
        int nData = 0;
        while (nData < dataLen)
        {
            // initialize variables for noise, the amount of modes and output s
            float noise = (float)RandomNumber.NextDouble() * 2f - 1f;
            //int bWGSize = ds.Length;
            float s;

            // Pointer p for the buffers. Index n equals p except when delayed
            int p = index % 20;
            int n = p;

            // Sum of the values that are multiplied by the b0 damping value
            float b0Sum = 0;
            for (int i = 0; i < bWGSize; i++)
            {
                zF[i, n] = sProc.directForm(zS[dex(n - ds[i])], zS[dex(n - 1 - ds[i])], zS[dex(n - 2 - ds[i])], bps[i], zF[i, dex(n - 1)], zF[i, dex(n - 2)], gamma[i]);
                b0Sum += zF[i, n];
            }

            // Sum of the values that are multiplied by the b1 damping value
            n = dex(n - 1);
            float b1Sum = 0;
            for (int i = 0; i < bWGSize; i++)
            {
                zF[i, n] = sProc.directForm(zS[dex(n - ds[i])], zS[dex(n - 1 - ds[i])], zS[dex(n - 2 - ds[i])], bps[i], zF[i, dex(n - 1)], zF[i, dex(n - 2)], gamma[i]);
                b1Sum += zF[i, n];
            }

            // Sum of the full equation.
            n = p;

            // Output Modal loop filters (bandpass with extra delays)
            float[] damps = sProc.getDampings2(dampings, index, sampleRate);
            s = damps[0] * (noise + b0Sum) + damps[1] * (delayN + b1Sum) - damps[2] * zS[dex(n - 1)];

            // Save current values
            zS[p] = s;
            delayN = noise;

            // LowPass
            zF[bWGSize, n] = sProc.directForm(zS[n], zS[dex(n - 1)], zS[dex(n - 2)], lp, zF[bWGSize, dex(n - 1)], zF[bWGSize, dex(n - 2)]);
            s = zF[bWGSize, n];

            // Output to all channels
            for (int i = 0; i < channels; i++)
                data[nData * channels + i] = impact * normalizeAmplifier * s;

            // Increment
            nData++;
            index++;
        }

        if (index > decayTime)
        {
            isRunning = false;
            //Destroy(this);
        }
    }

    public void resetBanded(float impact)
    {
        this.impact = impact;
        index = 0;
        isRunning = true;

        delayN = 0f;
        zS = new float[20];
        zF = new float[bWGSize + 1, 20];
    }

    // Creates a BandedMaterial class with specific values from the material to the model 
    public static void createBanded(GameObject thisParent, Materials.bandedWaveguideData bm, float impact)
    {
        // Only works if values for frequencies, delays, Qs and gamma are of the same size (Length)
        if (bm.bpFrequencies.Length == bm.bpDelays.Length && bm.bpDelays.Length == bm.bpQs.Length && bm.bpQs.Length == bm.bpGamma.Length)
        {
            // Adds the Banded class (this script) to this object
            EmitterBanded thisObj = thisParent.AddComponent<EmitterBanded>();

            GameObject go = GameObject.Find("MaterialCreater");
            thisObj.sProc = go.GetComponent<SProc>();

            thisObj.normalizeAmplifier = bm.normalizeAmplifier;
            thisObj.impact = impact; 
            thisObj.decayTime = bm.maxDecayTime;
            thisObj.materialName = bm.material;

            // Get values from the bandedWaveguideData and apply them to this object
            thisObj.ds = bm.bpDelays;
            thisObj.bWGSize = thisObj.ds.Length;
            thisObj.sampleRate = AudioSettings.outputSampleRate;
            thisObj.dampings = bm.dampings;
            thisObj.gamma = bm.bpGamma;
            thisObj.bps = thisObj.sProc.getBPFMatrix(bm.bpFrequencies, bm.bpQs, thisObj.sampleRate);
            thisObj.lp = thisObj.sProc.getLPF(bm.lpCutoff, bm.lpQ, thisObj.sampleRate);

            // Buffers for old data
            thisObj.zS = new float[20];
            thisObj.zF = new float[thisObj.bWGSize + 1, 20];
        }
        else
        {
            Debug.Log("Not equal paramters for modes");
        }
    }
    // Properties.
    private int bWGSize;
    private float[] dampings;
    private int[] ds;
    private float[] gamma;
    private float[][] bps;
    private float[] lp;

    private float sampleRate;

    // Initialized values for equations
    private float delayN;
    private float[] zS;
    private float[,] zF;

    // AUXILARY FUNCTIONS //
    int dex(int x)
    {
        if (x < 0)
            return 20 + x;
        else return x;
    }
}
