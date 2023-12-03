noise = 2*rand(4000,1)-1;
% Extended Karplus-Strong
Fs = 48000;
len = 12*Fs;
f0 = 440;

N1 = Fs/f0;
N = floor(Fs/f0-1);
Nr = Fs/f0-N-1;
X = [noise(1:N); zeros(len-N,1)];

Y = zeros(len,1);

%%%%%%%%%%%%%%
% PARAMETERS %
%%%%%%%%%%%%%%
outDel = zeros(len,1);
% Pick Direction lowpass
p = 0.9; outPickDir = zeros(len,1);

% Pick-Position comb filter
beta = 0.13; combDel = floor(beta*N); outPickPos=zeros(len,1);

% Damping second order lowpass
B = 1.0; sec = 14; rho = 0.001^(1/(f0*sec));
h0 = (1+B)/2; h1 = (1-B)/4; outDamp = zeros(len,1);

% Stiffness allpass

% String-Tuning allpass
%minST = -1/11; maxST = 2/3; nt = minST + Nr*(maxST-minST);
outStringTuning = zeros(len,1); nt = Nr;

% Energetic dynamic lowpass
w = (pi*f0)/Fs; enerC0 = w/(1+w); enerC1 = ((1-w)/(1+w)); L = 1.00; L0 = L^(1/3);
outEner = zeros(len,1);

% INDEPENDENT
% OverDrive
drive = 0.0; pregain = 10^(2*drive); lo = -1;
Ydist = zeros(len,1); 

% Wah-Wah https://ccrma.stanford.edu/~orchi/Documents/DAFx.pdf
wahF1 = 2*sin((pi*1000)/Fs); delWahY = 0; delWahlow = 0; Fc = zeros(len,1);
wahWet = 0.0;

for n=1:N
    outPickDir(n) =  (1-p) * X(n) + p*del(outPickDir,n-1);
    
    outPickPos(n) = outPickDir(n) - del(outPickDir,n-combDel);
end


for n=(N+1):len
    
    %%% DEPENDS ON N %%%
    outDel(n) = del(outPickPos,n-N) + del(outStringTuning, n-N);
    
    outDamp(n) = rho*(h1*(del(outDel, n)+del(outDel, n-2)) + h0*del(outDel, n-1));
    outStringTuning(n) = (1-nt)*outDamp(n) + nt*del(outDamp, n-1);
    
    outEner(n) = enerC0*(del(outDel,n)+ del(outDel,n-1))+enerC1*del(outEner, n-1);
    outEner(n) = L*L0*del(outDel,n)+(1-L)*outEner(n);
    Y(n-N) = outEner(n);
    
    %%% INDEPENDENT PART %%%
    x(n) = outEner(n);
    % Wah
    delWahY = wahF1*(x(n) - delWahlow - 0.4*delWahY) + delWahY;
    outWah(n) = wahWet*delWahY + (1-wahWet)*x(n);
    delWahlow = wahF1*outWah(n) + delWahlow;
    
    x(n) = outWah(n);
    
    % Overdrive
    x(n) = x(n)*pregain;
    x(n) = symclip(x(n));

    Ydist(n-N) = x(n); 
end

% Debug plot
k = 100;

sound(x,Fs);
%plot(wahX-outWah);

figure(2)
subplot(2,1,1)
plot(linspace(0,sec/2,len),Y);
subplot(2,1,2)
plot(linspace(0,sec/2,len),Ydist);

figure(1)
subplot(3,2,1)
plot(outPickDir)
xlim([1 N*k])
ylim([-1 1])
title('Pick Direction')

subplot(3,2,2)
plot(outPickPos)
xlim([1 N*k])
ylim([-1 1])
title('Pick Position')

subplot(3,2,3)
plot(outDel)
xlim([1 N*k])
ylim([-1 1])
title('Summation')

subplot(3,2,4)
plot(outDamp)
xlim([1 N*k])
ylim([-1 1])
title('Damping')

subplot(3,2,5)
plot(outEner)
xlim([1 N*k])
ylim([-1 1])
title('Level')

subplot(3,2,6)
plot(Ydist)
xlim([1 N*k])
ylim([-1 1])
title('Output')

audiowrite('ksTest.wav',Y,Fs);


%% Without drive and wah wah
Y = zeros(len,1);

%%%%%%%%%%%%%%
% PARAMETERS %
%%%%%%%%%%%%%%
outDel = zeros(len,1);
% Pick Direction lowpass
p = 0.9; outPickDir = zeros(len,1);

% Pick-Position comb filter
beta = 0.13; combDel = floor(beta*N); outPickPos=zeros(len,1);

% Damping second order lowpass
B = 0.8; S = B/2; sec = 2; rho = 0.001^(1/(f0*sec));
h0 = (1+B)/2; h1 = (1-B)/4; outDamp = zeros(len,1);

% Energetic dynamic lowpass
w = (pi*f0)/Fs; enerC0 = w/(1+w); enerC1 = ((1-w)/(1+w)); L = 0.2; L0 = L^(1/3);
outEner = zeros(len,1);

% Real-Time simulation
bufferSize = 256; iterations = floor(len/bufferSize); index = 1;

% On Pick
for n=1:N
    outPickDir(n) =  (1-p) * X(n) + p*del(outPickDir,n-1);
    
    outPickPos(n) = outPickDir(n) - del(outPickDir,n-combDel);
end

% Each Buffer
for iter=0:iterations-2
    
    Nbuffer = N+iter*bufferSize;
    
    for n=Nbuffer+1:Nbuffer+bufferSize
        
        outDel(n) = del(outPickPos,n-N) + del(outDamp, n-N);
        
        outDamp(n) = rho*(h1*(del(outDel, n)+del(outDel, n-2)) + h0*del(outDel, n-1));
        
        outEner(n) = enerC0*(del(outDel,n)+ del(outDel,n-1))+enerC1*del(outEner, n);
        outEner(n) = L*L0*del(outDel,n)+(1-L)*outEner(n);
        Y(n-N) = outEner(n);
        
    end
end

% Debug plot
k = 10;

soundsc(Y,Fs);

figure(2)
plot(linspace(0,2,len),Y);

figure(1)
subplot(3,2,1)
plot(outPickDir)
xlim([1 N*k])
ylim([-1 1])
title('Pick Direction')

subplot(3,2,2)
plot(outPickPos)
xlim([1 N*k])
ylim([-1 1])
title('Pick Position')

subplot(3,2,3)
plot(outDel)
xlim([1 N*k])
ylim([-1 1])
title('Summation')

subplot(3,2,4)
plot(outDamp)
xlim([1 N*k])
ylim([-1 1])
title('Damping')

subplot(3,2,5)
plot(outEner)
xlim([1 N*k])
ylim([-1 1])
title('Level')

subplot(3,2,6)
plot(Y)
xlim([1 N*k])
ylim([-1 1])
title('Output')
%% Functions
function [out] = del(p_Y, p_del)
if (p_del) < 1
    out = 0;
else
    out = p_Y(p_del);
end
end
