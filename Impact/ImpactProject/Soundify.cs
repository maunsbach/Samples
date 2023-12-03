using UnityEngine;

public class Soundify : MonoBehaviour
{
    private Materials materials;
    private int materialNumber;

    private bool isBanded;

    public SoundingObject soundingObject;
    public enum SoundingObject
    {
        ResonatorOnly, HammerOnly, ResonatorAndHammer
    }

    public float hammerElasticConstant = 5e11f;

    // MODEL SELECTING LIST
    public ModelList modelSelect;
    public enum ModelList
    {
        None, SpringMass, BandedWaveguide
    }

    // BANDED WAVEGUIDE MATERIAL SELECT
    public MaterialList materialList;
    public enum MaterialList
    {
        Nothing, Wood, Metal, Cardboard, Plastic, Glass
    }

    public bool rollable;

    // Add sound-emitting component to object
    void Awake()
    {
        GameObject go = GameObject.Find("MaterialCreater");
        materials = go.GetComponent<Materials>();

        materialNumber = getMaterialNumber(materialList.ToString());


        switch (soundingObject.ToString())
        {
            case "ResonatorOnly":
                if (modelSelect.ToString() == "BandedWaveguide")
                    isBanded = true;
                if (modelSelect.ToString() == "SpringMass")
                    StaticSoundingObject.createSM(gameObject, materials.springMassMaterialsPresets[materialNumber]);
                break;
            case "HammerOnly":
                if (rollable)
                {
                    EmitterHammerImpactRolling emitter1 = gameObject.AddComponent<EmitterHammerImpactRolling>();
                    emitter1.k = hammerElasticConstant;
                }
                else
                {
                    gameObject.AddComponent<EmitterHammerImpact>();
                }
                break;

            case "ResonatorAndHammer":
                if (modelSelect.ToString() == "BandedWaveguide")
                    isBanded = true;
                if (modelSelect.ToString() == "SpringMass")
                    StaticSoundingObject.createSM(gameObject, materials.springMassMaterialsPresets[materialNumber]);

                if (rollable)
                {
                    EmitterHammerImpactRolling emitter2 = gameObject.AddComponent<EmitterHammerImpactRolling>();
                    emitter2.k = hammerElasticConstant;
                }
                else
                {
                    gameObject.AddComponent<EmitterHammerImpact>();
                }
                break;
            default:
                Debug.Log("Select sounding object type");
                break;
        }

    }

    int getMaterialNumber(string m)
    {
        if (m == "Wood")
            return 0;
        else if (m == "Metal")
            return 1;
        else if (m == "Cardboard")
            return 2;
        else if (m == "Plastic")
            return 3;
        else if (m == "Glass")
            return 4;
        else return 0;

    }

    void OnCollisionEnter(Collision col)
    {
        if (isBanded)
        {
            EmitterBanded emitterBanded = col.gameObject.GetComponent<EmitterBanded>();

            float impact = 0.2f * Vector3.Dot(col.relativeVelocity, col.contacts[0].normal);

            if (emitterBanded == null)
            {
                EmitterBanded.createBanded(col.gameObject, materials.bandedMaterialsPresets[materialNumber], impact);
            }
            else
            {
                if (emitterBanded.materialName == materials.bandedMaterialsPresets[materialNumber].material)
                {
                    emitterBanded.resetBanded(impact);
                }
                else
                {
                    Destroy(emitterBanded);
                    EmitterBanded.createBanded(col.gameObject, materials.bandedMaterialsPresets[materialNumber], impact);

                }

            }


        }
    }
}
