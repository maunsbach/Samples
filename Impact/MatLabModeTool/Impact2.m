%% 
clc; clear;

% Load File
[y, Fs] = audioread('last 7.wav');


% Get frequency Domain
[Y, f]= fftHelper(y, Fs);

% Plot Frequency Domain
figure(1)
plot(f,Y);
ylabel('Gain');
xlabel('Frequency');
set(gca,'FontSize',16);
grid on

% Values for peakfidning (tweak for better peaks
%syms pks;
pks = [];
MPDistance = 100;       % Minimum distance between peaks
MPWidth = 50;           % Minimum width of a peak
noPeaks = 8;            % Number of peaks to find
freqLimit = 6000;       % The upper limit to look at
freqLimitIndex = find(f > freqLimit, 1, 'first');

while size(pks) < noPeaks
    [pks,locs,w,p]=findpeaks(Y(1:freqLimitIndex),f(1:freqLimitIndex),'NPeaks',noPeaks,'MinPeakDistance',MPDistance,'MinPeakWidth', MPWidth,'SortStr','descend');
    text(locs+.02,pks,num2str((1:numel(pks))'));
    xlim([0 freqLimit])
end

% Find Bandwidth with minimum of right and lefgoing (not exact)
bw = zeros(1,noPeaks);
for i=1:noPeaks
    idx = find(f == locs(i));
    
    % Rightgoing
    peakR = pks(i);
    idxR = idx;
    while peakR > pks(i) - 3
        peakR = Y(idxR + 1);
        idxR = idxR + 1;
    end
    
    % Leftgoing
    peakL = pks(i);
    idxL = idx;
    while peakL > pks(i) - 3
        peakL = Y(idxL - 1);
        idxL = idxL - 1;
    end
    
    bwR = 2*(f(idxR) - f(idx));
    bwL = 2*(f(idx) - f(idxL));
    
    bw(i) = min(bwR, bwL); 
end

%% Remove residuals and plot
residual = y;
for i=1:noPeaks
      
        freq = locs(i);  % estimated peak frequency in Hz
        bandwidth = bw(i);        % peak bandwidth estimate in Hz

        R = exp( - pi * bandwidth / Fs);            % pole radius
        z = R * exp(j * 2 * pi * freq / Fs); % pole itself
        B = [1, -(z + conj(z)), z * conj(z)] % numerator
        r = 0.8;     % zero/pole factor (notch isolation)
        A = B .* (r .^ [0 : length(B)-1]);   % denominator

        residual = filter(B,A,residual); % apply inverse filter
end

% Get frequency Domain
[Y, f]= fftHelper(residual, Fs);

figure(2)
plot(f,Y);
ylabel('Gain');
xlabel('Frequency');
set(gca,'FontSize',16);
xlim([0 freqLimit])
grid on
