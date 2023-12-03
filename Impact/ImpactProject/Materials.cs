using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Materials : MonoBehaviour
{

    //public springMassData[] springMassMaterials;
    //public bandedWaveguideData[] bandedMaterials;

    //public bool materialsReady = false;

    // The struct to keeps the parameters needed from the material
    [System.Serializable]
    public struct springMassData
    {
        public string material;
        [Range(1, 10)]
        public int numberOfModes;

        public float[] modes;
        public float q;
        public float[] mass;

        public float normalizeAmplifier;
        public int maxDecayTime;
    }

    // The struct to keeps the parameters needed from the material
    [System.Serializable]
    public struct bandedWaveguideData
    {
        public string material;
        [Range(1, 10)]
        public int numberOfModes;
        public float[] dampings;

        public float[] bpFrequencies;
        public int[] bpDelays;
        public float[] bpQs;
        public float[] bpGamma;

        public float lpCutoff;
        public float lpQ;

        public float normalizeAmplifier;
        public int maxDecayTime;
    }

    // OnValidate updates each time a public value is changed in the inspecter.
    // Ensures the same amount of frequencies, delays, Qs and gamma
    /*void OnValidate()
    {
        // Checks for Spring-Mass
        int len = springMassMaterials.Length;
        if (len != 0)
        {

            for (int i = 0; i < len; i++)
            {
                int size = springMassMaterials[i].numberOfModes;

                if (springMassMaterials[i].modes.Length != size)
                {
                    float[] temp = new float[size];
                    int maxSize = Mathf.Min(springMassMaterials[i].modes.Length, size);
                    for (int j = 0; j < maxSize; j++)
                    {
                        temp[j] = springMassMaterials[i].modes[j];
                    }
                    springMassMaterials[i].modes = temp;
                }

            }
        }

        // Check for Banded
        len = bandedMaterials.Length;
        if (len != 0)
        {

            for (int i = 0; i < len; i++)
            {
                int size = bandedMaterials[i].numberOfModes;
                if (bandedMaterials[i].dampings.Length != 3)
                    bandedMaterials[i].dampings = new float[3];

                if (bandedMaterials[i].bpFrequencies.Length != size)
                {
                    float[] temp = new float[size];
                    int maxSize = Mathf.Min(bandedMaterials[i].bpFrequencies.Length, size);
                    for (int j = 0; j < maxSize; j++)
                    {
                        temp[j] = bandedMaterials[i].bpFrequencies[j];
                    }
                    bandedMaterials[i].bpFrequencies = temp;
                }

                if (bandedMaterials[i].bpDelays.Length != size)
                {
                    int[] temp = new int[size];
                    int maxSize = Mathf.Min(bandedMaterials[i].bpDelays.Length, size);
                    for (int j = 0; j < maxSize; j++)
                    {
                        temp[j] = bandedMaterials[i].bpDelays[j];
                    }
                    bandedMaterials[i].bpDelays = temp;
                }

                if (bandedMaterials[i].bpQs.Length != size)
                {
                    float[] temp = new float[size];
                    int maxSize = Mathf.Min(bandedMaterials[i].bpQs.Length, size);
                    for (int j = 0; j < maxSize; j++)
                    {
                        temp[j] = bandedMaterials[i].bpQs[j];
                    }
                    bandedMaterials[i].bpQs = temp;
                }

                if (bandedMaterials[i].bpGamma.Length != size)
                {
                    float[] temp = new float[size];
                    int maxSize = Mathf.Min(bandedMaterials[i].bpGamma.Length, size);
                    for (int j = 0; j < maxSize; j++)
                    {
                        temp[j] = bandedMaterials[i].bpGamma[j];
                    }
                    bandedMaterials[i].bpGamma = temp;
                }
            }
        }
    }*/

    // PRESETS Spring-Mass
    public springMassData[] springMassMaterialsPresets = new springMassData[]
        {
        new springMassData
        {
            material = "Wood",
            modes = new float[]{ 180f, 200f, 400f, 520f, 690f },
            numberOfModes = 5,
            q = 200,
            mass = new float[] {0.005f, 0.005f, 0.005f, 0.005f,  0.005f },

            normalizeAmplifier = 80f,
            maxDecayTime = 2*48000
        },

        new springMassData
        {
            material = "Metal",
            modes = new float[]{ 901f,    1675f,    2424f,    3082f,    3473f,    4297f,    4649f,    5093f,    5497f,    5712f,    6339f },
            numberOfModes = 11,
            q = 500,
            mass = new float[] {10f,    0.9f,    0.05f,    0.9f,    0.476190476f,    0.6f,    0.2f,    0.1f,    5f,    5f,    5f},

            normalizeAmplifier = 4000f,
            maxDecayTime = 2*48000
        },

        new springMassData
        {
            material = "Cardboard",
            modes = new float[]{ 270.5929168f, 49.34341425f, 741.7429367f, 579.3871866f, 1298.846001f, 1550.338241f, 1949.860724f,  2137.684043f, 3127.735774f },
            numberOfModes = 9,
            q = 9,
            mass = new float[] {5f, 25f, 0.5f, 0.5f, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 0.5f},

            normalizeAmplifier = 4000f,
            maxDecayTime = 2*48000
        },

        new springMassData
        {
            material = "Plastic",
            modes = new float[]{ 250.1085541f, 614.8501954f, 1031.697785f, 1302.648719f,   1698.65393f,    2198.871038f,    2626.139818f, 2980.460269f, 7478f, 8015f, 111560f},
            numberOfModes = 11,
            q = 5,
            mass = new float[] {10f, 1f, 1f, 1f, 2f, 5f, 0.1f, 0.05f , 0.1f, 0.05f, 0.1f},

            normalizeAmplifier = 8000f,
            maxDecayTime = 2*48000
        },

        new springMassData
        {
            material = "Glass",
            modes = new float[]{ 1951.281f, 3507.794f,  1131.668f,  2902.483f,  4161.98f,   3996.554f,  4827.446f,  3188.22f},
            numberOfModes = 8,
            q = 400,
            mass = new float[] {0.005f, 0.005f, 0.005f, 0.005f,  0.005f, 0.005f, 0.005f,  0.005f},

            normalizeAmplifier = 200f,
            maxDecayTime = 3*48000
        },

        /*new springMassData
        {
            material = "Plank",
            modes = new float[]{748.7219819f,   1518.11696f,    985.1131959f,   1253.862143f,   2327.959103f,   1800.348295f,   2495.140722f,   2016.066513f,   3299.589911f,   2666.816471f,   319.1f},
            numberOfModes = 11,
            q = 40,
            mass = new float[] {0.005f, 0.005f, 0.005f, 0.005f,  0.005f, 0.005f, 0.005f,  0.005f, 0.005f, 0.005f,  0.005f}
        }*/

        };

    // Preset BANDED
    public bandedWaveguideData[] bandedMaterialsPresets = new bandedWaveguideData[]
    {
        new bandedWaveguideData
        {
            material = "Wood",
            dampings = new float[]{30, 50, 40},
            bpFrequencies = new float[] {748, 1518, 985, 1253, 2327, 1800, 2495  },
            numberOfModes = 7,
            bpDelays = new int[] {3,3,3,3,3,3,3 },
            bpQs = new float[] {14,16,16,14,16, 12, 17},
            bpGamma = new float[] {5, 5, 4, 4, 2, 2, 2},

            lpCutoff = 3200,
            lpQ = 1,

            normalizeAmplifier = 0.1f,
            maxDecayTime = 48000
        },

        new bandedWaveguideData
        {
            material = "Metal",
            dampings = new float[]{120, 16, 6},
            bpFrequencies = new float[] {901, 1675, 2145, 2424, 2758, 3082, 3473, 4297 },
            numberOfModes = 8,
            bpDelays = new int[] {3,3,3,3,3,3,3,3 },
            bpQs = new float[] {14,16,16,14,16, 12, 17, 16},
            bpGamma = new float[] {9, 9, 6, 6, 3, 2, 2, 1 },

            lpCutoff = 3200,
            lpQ = 1,

            normalizeAmplifier = 0.1f,
            maxDecayTime = 48000
        },

        new bandedWaveguideData
        {
            material = "Cardboard",
            dampings = new float[]{120, 60, 40},
            bpFrequencies = new float[] {964, 211, 1438, 720, 1785, 4100, 3721, 4406  },
            numberOfModes = 8,
            bpDelays = new int[] {3,3,3,3,3,3,3,3 },
            bpQs = new float[] {14,16,16,14,16,14,16,12 },
            bpGamma = new float[] {9, 8, 4, 4, 2, 2, 2, 1 },

            lpCutoff = 4600,
            lpQ = 1,

            normalizeAmplifier = 0.1f,
            maxDecayTime = 48000
        },

        new bandedWaveguideData
        {
            material = "Plastic",
            dampings = new float[]{180, 150, 20},
            bpFrequencies = new float[] {614, 250, 1302, 1031, 2980, 1698, 2198, 2626  },
            numberOfModes = 8,
            bpDelays = new int[] {3,3,3,3,3,3,3,3 },
            bpQs = new float[] {18,19,10,14,16, 12, 17, 10 },
            bpGamma = new float[] {3, 2, 2, 2, 3, 2, 2, 1 },

            lpCutoff = 2900,
            lpQ = 1,

            normalizeAmplifier = 0.1f,
            maxDecayTime = 48000
        },



        new bandedWaveguideData
        {
            material = "Glass",
            dampings = new float[]{80, 20, 5},
            bpFrequencies = new float[] {733, 1951, 3507, 1131, 2902, 4161, 3996, 4827 },
            numberOfModes = 8,
            bpDelays = new int[] {3,3,3,3,3,3,3,3 },
            bpQs = new float[] {18,19,10,14,16, 12, 17, 10 },
            bpGamma = new float[] {9, 8, 7, 6, 3, 2, 2, 5 },

            lpCutoff = 5200,
            lpQ = 1,

            normalizeAmplifier = 0.1f,
            maxDecayTime = 2*48000
        }
    };


    void Start()
    {
        Profiler.logFile = "mylog.txt"; //Also supports passing "myLog.raw"
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;

    }
}
