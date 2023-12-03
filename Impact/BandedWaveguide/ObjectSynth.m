%% Digital Waveguide of various materials
% Wood
fs = 48000;
sec = 1;
dampings = [30 50 40];
Fs = [180 200 400 520 690];
B = [3 3 2 2 2];
ds = [5 5 3 4 5];
gamma = [8 6 3 2 2];
cutoff = 2000;

[s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, gamma, cutoff);
soundsc(s,fs)
%audiowrite('SynthWood.wav',s,fs)
%% Spectogram of s

spectrogram(s,3000,2000,20000,fs,'yaxis')
view(5,34)
ylim([0.05 1])
%xlim([0 1])
%set(gca,'Ydir','reverse')    
set(gca,'FontSize',24)
shading interp
colormap gray
colorbar off

%% Carboard
fs = 48000;
sec = 1;
dampings = [100 110 40];
Fs = [109 230 352 413];
B = [1 0.5 0.5 1];
ds = [1 1 2 2];
lambda = [16 10 3 5];
cutoff = 500;

[s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, lambda, cutoff);
%audiowrite('SynthCardboard.wav',s,fs)
soundsc(s,fs)

%% China
fs = 48000;
sec = 2;
dampings = [15 34 2];
Fs = [1245 1503 2131 2575];
B = [20 24 28 36];
ds = [5 5 3 4];
lambda = [12 10 8 10];
cutoff = 3000;

[s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, lambda, cutoff);
%audiowrite('SynthChina.wav',s,fs)
soundsc(s,fs)

%% Glass 1
fs = 48000;
sec = 2;
dampings = [14 12 12];
Fs = [1233 1679 4251 4640];
B = [20 34 35 26];
ds = [4 4 2 1];
lambda = [7 8 10 4];
cutoff = 6000;

[s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, lambda, cutoff);
%audiowrite('SynthGlass1.wav',s,fs)
soundsc(s,fs)

%% Glass 1
fs = 48000;
sec = 2;
dampings = [12 12 6];
Fs = [2312 2466 3200 5314];
B = [20 34 35 26];
ds = [4 3 2 1];
lambda = [7 7 5 2];
cutoff = 6000;

[s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, lambda, cutoff);
%audiowrite('SynthGlass2.wav',s,fs)
soundsc(s,fs)

%% Metal
fs = 48000;
sec = 2;
dampings = [15 16 6];
Fs =     [3454 5645 6433 6999];
B =      [16 20 20 16];
ds =     [3 1 1 1];
lambda = [6 1.5 2.5 3];
cutoff = 7500;

[s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, lambda, cutoff);
%audiowrite('SynthMetal.wav',s,fs)
soundsc(s,fs)

%% Unity testing 3 freq
% Wood
fs = 48000;
sec = 1;
dampings = [30 50 40];
Fs = [180 200 400];
B = [2 4 2];
ds = [1 1 1];
lambda = [1 1 1];
cutoff = 2000;

[s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, lambda, cutoff);
soundsc(s,fs)

%% UNITY testing 1 freq
% Wood
fs = 100;
sec = 1;
dampings = [30 50 40];
Fs = [180];
B = [2];
ds = [1];
lambda = [1];
cutoff = 2000;

[s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, lambda, cutoff);
soundsc(s,fs)

%%
[x, fs] = audioread('metalhollow.wav');

plot(x)

soundsc(x,fs)