function [ Y ] = fftHelper2(s, fs, sec)
N = length(s);
f = fs/N .* (0:N-1); 
Y = fft(s, N); 
Y = abs( Y(1:N) ) ./ (N/2);
Y(Y==0) = eps;
Y = 20 * log10(Y);
Y = Y-min(Y);

Y = resample(Y, 1, sec);
end

