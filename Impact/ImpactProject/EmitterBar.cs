using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EmitterBar : MonoBehaviour
{
    private System.Random RandomNumber = new System.Random();
    public bool isImpacting = false;
    private float normalizeAmplifier = 200f;
    public float maxIt = 0;

    // IMPACT
    private int maxDecayTime;
    private int decayTime;
    private bool impacting;

    // New values on hit
    private bool newHit;
    private float[] newbx;
    private float[] newbv;
    private float newK1;
    private float newK2;


    public void HitBar(float[] posMass)
    {

        float tempsumbx = 0f;
        float tempsumbv = 0f;
        for (int i = 0; i < len; i++)
        {
            newbx[i] = detM[i] * 1 / posMass[i];
            tempsumbx += newbx[i];
            newbv[i] = detM[i] * (alpha / posMass[i]);
            tempsumbv += newbv[i];
        }

        newK1 = bhx - tempsumbx;
        newK2 = bhv - tempsumbv;

        decayCounter = 0;
        newHit = true;
        impacting = true;
        isImpacting = true;
    }

    void HitUpdate()
    {
        bx = newbx;
        bv = newbv;

        K1 = newK1;
        K2 = newK2;

        fTot = 0;
        outhv = 200;
        outhx = 0;
        outx = new float[len];
        outv = new float[len];
        newHit = false;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (newHit)
            HitUpdate();

        if (!isImpacting)
            return;

        // The length of the data = 1024
        int dataLen = data.Length / channels;

        // Counter for for each clip of data
        int nData = 0;
        while (nData < dataLen)
        {
            float s = 0f;

            if (isImpacting)
            {
                ///////////////////////
                // Impact
                float sumoutx = 0;
                float sumoutv = 0;

                for (int j = 0; j < len; j++)
                {
                    float outxTemp = A1x[j] * outx[j] + A2x[j] * outv[j] + bx[j] * fPrev;
                    outv[j] = A1v[j] * outx[j] + A2v[j] * outv[j] + bv[j] * fPrev;
                    outx[j] = outxTemp;

                    sumoutx += outx[j];
                    sumoutv += outv[j];
                }

                float outhxTemp = Ah1x * outhx + Ah2x * outhv + bhx * fTot;
                outhv = Ah1v * outhx + Ah2v * outhv + bhv * fTot;
                outhx = outhxTemp;

                float h0 = fPrev;

                // Newton Rhapson
                if (impacting)
                {

                    float p1 = outhx - sumoutx;
                    float p2 = outhv - sumoutv;

                    int count = 0;
                    float err = 99;

                    for (int maxIterations = 0; maxIterations < 20; maxIterations++)
                    {

                        float xTi = p1 + K1 * h0;

                        if (xTi < 0)
                        {
                            h0 = 0;
                            err = 0;
                            impacting = false;
                        }
                        else
                        {
                            float vTi = p2 + K2 * h0;

                            float gNR = Mathf.Pow(xTi, a) * (k + lambda * vTi) - h0;

                            float gNRDer = a * Mathf.Pow(xTi, (a - 1)) * (k + lambda * vTi) * K1 + lambda * Mathf.Pow(xTi, a) * K2 - 1;

                            float h1 = h0 - gNR / gNRDer;

                            count = count + 1;
                            maxIt = Mathf.Max(count, maxIt);

                            err = Mathf.Abs(h1 - h0);

                            if (err < errMax)
                                break;

                            h0 = h1;
                        }
                    }
                    if ((p1 + K1 * h0) < 0)
                    {
                        h0 = 0;
                        impacting = false;
                    }
                }
                // Newton Rhapson over and out
                fTot = h0 - mh * grav;

                s = 0;
                for (int j = 0; j < len; j++)
                {
                    outx[j] = outx[j] + bx[j] * h0;
                    outv[j] = outv[j] + bv[j] * h0;

                    s += outx[j];
                }

                outhx = outhx + bhx * fTot;
                outhv = outhv + bhv * fTot;
                fPrev = h0;

                decayCounter++;
            }

            ///////////////////////
            // Output to all channels
            for (int i = 0; i < channels; i++)
                data[nData * channels + i] = s * normalizeAmplifier;

            // Increment
            nData++;
  
        }

        if (decayCounter > decayTime)
            isImpacting = false;
    }

    // MODEL PROPERTIES //
    // Upon start
    private float grav = 9.82f;
    private float errMax = 1e-13f;
    private float sampleRate;

    private int len;
    private int decayCounter;

    private float[] omega;
    private float[] g;
    private float a = 2.8f;

    private float[] detM;

    private float[] A1x;
    private float[] A2x;
    private float[] A1v;
    private float[] A2v;

    private float[] bx;
    private float[] bv;

    private float sumbx = 0;
    private float sumbv = 0;

    private float[] outx;
    private float[] outv;

    private float outhx;

    // After impact
    private float K1;
    private float K2;

    private float outhv;

    private float fPrev;
    private float fTot;

    public float k = 5e11f;
    private float mh = 0.001f;
    private float lambda;
    private float alpha;

    private float Ah1x;
    private float Ah2x;
    private float Ah1v;
    private float Ah2v;

    private float bhx;
    private float bhv;

    public springMassData SM;
    public float marimbaFundamental = 0f;

    [System.Serializable]
    public struct springMassData
    {
        public string material;

        public float[] modes;
        public float q;
        //public float[] mass;

        public float normalizeAmplifier;
        public int maxDecayTime;
    }

    /*[System.Serializable]
    public struct springMassData
    {
        public string material;
        //[Range(1, 10)]
        //public int numberOfModes;

        public float[] modes;
        public float q;
        public float[] mass;

        public float normalizeAmplifier;
        public int maxDecayTime;
    }*/

    // Use this for initialization
    void Awake()
    {
        float[] modes = new float[] { marimbaFundamental, marimbaFundamental * 3.95f, marimbaFundamental * 10, marimbaFundamental * 19.32f };
        //float[] modes = new float[] { marimbaFundamental, marimbaFundamental * 3.95f};

        //Debug.Log(modes[0]);

        //Debug.Log(modes[1]);

        alpha = 2f * AudioSettings.outputSampleRate;
        lambda = 0.6f * k;

        Ah1x = 1f;
        Ah2x = 2f / alpha;
        Ah1v = 0f;
        Ah2v = 1f;

        bhx = -1f / (Mathf.Pow(alpha, 2) * mh);
        bhv = -1f / (alpha * mh);

        //StaticSoundingObject thisObj = thisParent.AddComponent<StaticSoundingObject>();

        // Get values and apply them to this object
        if (marimbaFundamental > 0)
            len = modes.Length;
        else
            len = SM.modes.Length;

        Debug.Log(len);

        float sampleRate = AudioSettings.outputSampleRate;

        normalizeAmplifier = SM.normalizeAmplifier;

        float[] omega = new float[len];
        float[] g = new float[len];

        detM = new float[len];

        outx = new float[len];
        outv = new float[len];

        A1x = new float[len];
        A2x = new float[len];
        A1v = new float[len];
        A2v = new float[len];

        bx = new float[len];
        bv = new float[len];


        for (int i = 0; i < len; i++)
        {
            if (marimbaFundamental > 0)
                omega[i] = 2f * Mathf.PI * modes[i];
            else
                omega[i] = 2f * Mathf.PI * SM.modes[i];
            g[i] = omega[i] / SM.q;
            detM[i] = 1 / (Mathf.Pow(alpha, 2) + alpha * g[i] + Mathf.Pow(omega[i], 2));
            A1x[i] = detM[i] * (Mathf.Pow(alpha, 2) + alpha * g[i] - Mathf.Pow(omega[i], 2));
            A2x[i] = detM[i] * 2f * alpha;
            A1v[i] = detM[i] * (-2f * alpha * Mathf.Pow(omega[i], 2));
            A2v[i] = detM[i] * (Mathf.Pow(alpha, 2) - alpha * g[i] - Mathf.Pow(omega[i], 2));

            /*if (marimbaFundamental == 0)
            {
                bx[i] = detM[i] * 1 / SM.mass[i];
                sumbx += bx[i];
                bv[i] = detM[i] * (alpha / SM.mass[i]);
                sumbv += bv[i];
            }*/
        }

        decayTime = SM.maxDecayTime;

        newbx = new float[len];
        newbv = new float[len];
    }
}
