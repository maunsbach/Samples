# Instant Impact Sound Toolbox
This project led to the creation of a synthesis toolbox for impact and rolling sounds for the Unity game engine. Every game object with a rigidbody component can emit sound with these tools based on their physical properties.

An implementation of a Marimba bar and glass table ("EmmitterMarimba.cs") was created for the paper "Non-Linear Contact Sound Synthesis for Real-Time Audio-Visual Applications using Modal Textures".

---

## How-To
Import whole folder to unity project and follow these steps to add impact and rolling sound:

1. Add the Materials.cs script anywhere
2. Add the Soundify script to your to-be sounding object containing a Rigidbody
3. Select your soundify settings
---

## Which soundify settings to use?
Hammer Only:
The object does not characterize a sound, but emits a sound when interacting with a resonator.
Useful for bullets, balls, and when you want sound only from the object this strikes.

Resonator Only:
The resonator characterizes a material, and a sound is heard when struck by a hammer.
Useful for walls, floor and other non-moving objects.

Hammer and Resonator:
Sound is produced both when striking objects and when being struck.
Useful for moving plates and other moving objects that emit a characteristic sound.

## Which model to use?
Spring Mass:
Physically derived sound. Few parameters needed and the sound is affected by the hammers elastic coefficient. Whole model is affected by impact velocity.

Banded Waveguide
A burst of noise run through damping filters and bandpass filters. Only sound volume is affected by the impact.

## Other?
Material List: Select from a material from the presets.

Rollable: The object can roll and adds rolling sound on Spring Mass resonators.

Hammer Elastic Constant: Lower values creates softer impacts (default 5e11). 