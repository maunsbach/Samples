function [s] = BandedMaterial(fs, sec, dampings, Fs, B, ds, lambda, cutoff)
%% Digital Waveguide
% Generate noise -1 to 1.
len = sec*fs;
e = rand(len,1);
e = e*2 -1;

% DELETE!!!
%e = ones(fs,1);

% Create the exponential decaying functions for one pole filter
[b0, b1, a0] = ExpDecOnePole3(dampings, len, 48000);

% Create a zerobuffer head so the difference equation does hit indexes below
% 1 and have an error.
% zero Buffer Size
zBS = 100;
% zero Buffer Vector
zBV = zeros(zBS,1);
% Add zBV to all vectors used in the difference equation
s = [zBV; zeros(len,1)];
b0 = [zBV; b0];
b1 = [zBV; b1];
a0 = [zBV; a0];
e = [zBV; e];

% Coefficients for the bandpass filters. One for each frequency
C = BPCoeffs(Fs(1),B(1));
for i=2:length(Fs)
   C = [C;  BPCoeffs(Fs(i),B(i))];
end

% Banded waveguidez
[s] = digitalWaveguideS3(len, zBS, b0, b1, a0, C, ds, lambda, e);

% Remove zero buffer and low pass filter
s = s(zBS+1:end);

% Create butterworth filter
[b,a] = butter(4,cutoff/(fs/2));

% Run the dynamic filtered sound through the filter and plot
s = filter(b,a,s);
figure(1)
xTime = linspace(0,sec,len);
plot(xTime,s)
title('Time domain');
xlim([0 1])

% FFT
figure(2)
Y = fftHelper2(s, fs, sec);
plot(0:fs-1, Y(1:fs));
title('Frequency Domain of Impact Sound with Banded Waveguide')
xlim([0 cutoff]);
ylabel('Magnitude (dB)')
xlabel('Frequency Hz')
grid on;
end

