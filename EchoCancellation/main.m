%% LMS or NLMS
clc
clear sound;

% Mixed recording to load
[m, Fs] = audioread('FILENAME.wav');

% Signal to remove 
[x, Fs] = audioread('FILENAME.wav');

% filter length
p = 512;

% for LMS
mu = 0.01; 
[y, s] = LMS_Cancellation(m,x, p, mu);

% for NLMS
%mu = 2/(p*var(x2)); 
%[y, s] = NLMS_Cancellation(m2,x2, p, mu, 0.001);

win = [1 3000];

subplot(2,2,1)
plot(x)
xlim(win);
ylim([-1 1])
title({'Signal to Remove'});
%title({'Undesired Signal','karensVoice.wav'});
xlabel('Time in Samples')
ylabel('Amplitude')

subplot(2,2,3)
plot(s)
xlim(win);
ylim([-1 1])
title({['Filtered signal with p=' num2str(p)]});
%title({['Filtered signal with p ' num2str(p)],'Prediction of karensVoice.wav'});
xlabel('Time in Samples')
ylabel('Amplitude')

subplot(2,2,2)
plot(m)
xlim(win);
ylim([-1 1])
title({'Mixed Signal'})
%title({'Mixed Signal','recordingMixture.wav'})
xlabel('Time in Samples')
ylabel('Amplitude')

subplot(2,2,4)
plot(y)
xlim(win);
ylim([-1 1])
title({'Echo Cancelled Signal'})
%title({'Echo Cancelled Signal','Subtracted recordingMixture.wav'})
xlabel('Time in Samples')
ylabel('Amplitude')

soundsc(y,Fs)

