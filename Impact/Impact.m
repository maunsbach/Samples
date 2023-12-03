%% From Sounding Object chapter 8 
% Trapezoidal rule from slides "The ultimate guide to mass-spring systems"

% CONTINOUS-TIME MODEL for one center frequency
% dotdotx(t) + g*dotx(t) + omega^2*x(t) = 1/m * f

% TRAPEZOIDAL RULE TO DISTRETIZE
% x = x1
% dotx = dotx1 = x2
% dotdotx = dotx2

% REWRITTEN continous-time model
% dotx2 + g*x2 + omega^2*x1 = 1/m * f
% dotx2 = -omega^2*x1 - g*x2 + 1/m * f

% [dotx1; dotx2] = [0 1; omega^2 -g] * [x1 x2] + [0 1/m] * u      %(u = f)
% (in two lines) ->
% [dotx1] = [0        1] * [x1] + [0  ] * [u]
% [dotx2]   [-omega^2 -g]   [x2]   [1/m]   [u]

% TO UPDATE STATES
% x[n] = H((alpha * l + A) * x[n-1] + B(u[n] + u[n-1]))
% x[n] = [x1 x2] = [position velocity] ?

%% PARAMETERS
Fs = 48000;         % sampling rate
freq = [180, 200, 400, 520, 690];   % Oscillator Center Frequency (test)
modes = length(freq);               % Number of modes
m = ones(1,modes)*0.005;            % Modal mass (1 mode)
q = 200;                            % quality factor
T = 1/Fs;                           % Period

% For Contact Force
k = 5*10^11;
lambda = k*0.6;
a = 2.8;
alpha = 2*Fs;                    % alpha = 2/T
grav = 9.8;         % external acceleration on hammer

omega = zeros(1,modes);             
g = zeros(1,modes);       
A = zeros(2, 2*modes);
Atest = zeros(2, 2*modes);  
b = zeros(2, modes);
H = zeros(2, 2*modes);
C = zeros(2,modes);
D =[1 0;0 1];      
x = zeros(2,modes); 

for i=1:modes
    omega(i) = 2*pi*freq(i);
    g(i) = omega(i)/q; 
    tempA = [0 1;-omega(i)^2 -g(i)];
    Atest(1:2,i*2-1:i*2) = tempA;
    tempB = [0; 1/m(i)];
    H(1:2,i*2-1:i*2) = inv(alpha * eye(2) - tempA);
    A(1:2,i*2-1:i*2) = H(1:2,i*2-1:i*2) * (alpha*eye(2) + tempA);
    b(1:2,i) = H(1:2,i*2-1:i*2) * tempB;
    C(1:2,i) = tempB;
    %x(2,i) = 1;
end

mh = 1e-3;              % hammer mass

tempAh = [0 1;0 0];
tempCh = [0; -1/mh];
Ah = inv(alpha*eye(2)-tempAh)*(alpha*eye(2)+tempAh);
bh = inv(alpha*eye(2)-tempAh)*tempCh;

xh = [0; 2];

K = -(bh+sum(b,2));

Kme = K;

% Initial values 
f = zeros(1,Fs);
y = zeros(1, Fs);   % Sample output length 1 second
h0 = 0;
fPrev = 0;
fTot = 0;

errMax = 10^(-7);
maxIt = 0;

hs = zeros(1,Fs);

for t=1:Fs   
    for j=1:modes
       x(1:2,j) = A(1:2,j*2-1:j*2)*x(1:2,j) + b(1:2,j)*fPrev; 
    end
    
    xh = Ah*xh+bh*fTot;
    
    % Newton Rhapson
    p = xh - sum(x,2);
    count = 1;
    err = 99;
    
    while err > errMax && count < 20
        
        xTi = p(1)+K(1)*h0;
        
        if (xTi<0)   % Negative penetration => no contact force 
            h0 = 0;
            err = 0;
        else
            
        vTi = p(2)+K(1)*h0;
        
        gNR = xTi^a*(k + lambda * vTi) - h0;
        
        derX = a * xTi^(a-1)*(k+lambda*vTi)*K(1);
        derV = lambda*xTi^a*K(2)-1;
        gNRDer = derX + derV;
        
        h1 = h0 - gNR/gNRDer;
        
        count = count+1;
        maxIt = max(count, maxIt);

        err = abs(h1 - h0); 
        
        h0 = h1;
        end
        
    end
    hs(t) = count;
    if (p(1)+K(1)*h0<0)
        h0 = 0;
    end
    % Newton Rhapson over and out
    f(t) = h0; 
    
    fTot = h0 - mh * grav;
    
    %fTot = h0;
    
    %x = x + b*(h0 + fPrev);
    x = x + b*h0;
    xh = xh + bh*fTot; 
    fPrev = h0;
        
    %end
    y(t) = sum(x(1,:));
    
end

figure(1)
plot(y)

soundsc(y,Fs);