# Extended Karplus-Strong with Effects
Based on the algorithm by Karplus and Strong and extended by Smith [1,2] with added Wah Wah and Overdrive effects.

Components include

1. Low-Pass filter

2. Band-Pass filter

3. Linear interpolation

4. Comb filter

5. Dynamic Level filter

6. ... and more

### MatLab
The MatLab file can be run as a single offline instance of the synthesis. The first section includes the wah wah and drive, while the later is without.

### C++
The C++ code is implemented in Unity's Native Audio Plugin SDK framework to be built as a Unity Mixer plugin. It can then be controlled from within a Unity application.

### Sources

1. Karplus,  K.  and  Strong,  A.  “Digital  Synthesis  of  Plucked-String  and  DrumTimbres”. In:Computer Music Journal7 (June 1983), pp. 43–55.

2. Smith, J. O. “Virtual electric guitars and effects using FAUST and Octave”.In:Proceedings of the 6th International Linux Audio Conference (LAC-08)(2008).