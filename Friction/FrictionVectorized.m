% Clear
clear sound;

% Change of steady state bow velocity
Vb = 0.1;           % steady state bow velocity

% Frequency of resonator
%freq = [130.81, f0*a^12, f0*a^19];
freq = 100*(1:3);
modes = length(freq);                   % Number of mode
q_r = 500;                              % g = omega/q
oneMass = 1e-3;
m = ones(1,modes)*oneMass;              % Modal mass (1 mass)

Fs = 48000;                             % sampling rate

% Compute state transforms for resonator
omega_r = zeros(1,modes);
g_r = zeros(1,modes);

b_r = zeros(2,modes);
A_r = zeros(2,2,modes);
xv_r = zeros(2,modes);

for i=1:modes
    omega_r(i) = 2*pi*freq(i);
    g_r(i) = omega_r(i)/q_r;
    detTemp = (Fs^2+g_r(i)*Fs/2+omega_r(i)^2/4);
    
    A_r(1,1,i) = 1/detTemp * (detTemp-omega_r(i)^2/2);
    A_r(2,1,i) = 1/detTemp * (-Fs*omega_r(i)^2);
    A_r(1,2,i) = 1/detTemp * Fs;
    A_r(2,2,i) = 1/detTemp * (2*Fs^2-detTemp);
    
    b_r(1,i) = (1/m(i))*(1/(4*detTemp));
    b_r(2,i) = (1/m(i))*(1/(4*detTemp))*2*Fs;
end

% State transforms for the bow
q_b = 1; m_b = 50e-3;

b_b = zeros(2,1);
A_b = zeros(2,2);
xv_b = zeros(2,1);

omega_b = 0;
g_b = 0;
detTemp = (Fs^2+g_b*Fs/2+omega_b^2/4);
A_b(1,1) = 1/detTemp * (detTemp-omega_b^2/2);
A_b(2,1) = 1/detTemp * (-Fs*omega_b^2);
A_b(1,2) = 1/detTemp * Fs;
A_b(2,2) = 1/detTemp * (2*Fs^2-detTemp);
b_b(1) = (1/m_b)*(1/(4*detTemp));
b_b(2) = (1/m_b)*(1/(4*detTemp))*2*Fs;


% Force constants
sig0 = 10000; sig1 = .1*sqrt(sig0); sig2 = 0.4; sig3 = 0.0; v_s = 0.1;
mu_d = 0.2; mu_s = 0.4;  Zss = 0; c = 0.7;
f_N = 1; f_s = mu_s*f_N; f_c = mu_d*f_N; Zba = c*f_c/sig0;

fe_b=f_c +(f_s-f_c)*exp(-(Vb/v_s)^2) +sig2*Vb; % with w=0 and sgn(Vb)=1
% eq 3+4 Interactive Simulation of Rigid Body Interaction
% With Friction-Induced Sound Generation

% K components
K2 = 1/(2*Fs);

bv_r = sum(b_r,2);
bv_r = bv_r(2);
bv_b = b_b(2);
bv = bv_r+b_b(2);
K1 = -bv/(1+sig2*bv)*(sig0/(2*Fs)+sig1);

% Initial values
smplen = 4*Fs;
sig = zeros(smplen, 1);
yPrev = 0;
zPrev = 0;
z_Ti = 0;
f_tot_b=0;                  % total force on bow
f_tot_r=0;

errMax = 10^(-13);
hs = zeros(smplen,1);
h0 = 0;
maxIt = 0;
f_fr = zeros(smplen, 1);


% Compute for each sample
for t=1:smplen
    if (t == step)
        Vb = 0.3;
        fe_b=f_c +(f_s-f_c)*exp(-(Vb/v_s)^2) +sig2*Vb;
    end
    
    % Computable part of x and v without force
    for j=1:modes
        xv_r(:,j) = A_r(:,:,j)*xv_r(:,j) + b_r(:,j)*f_tot_r;
    end
    
    xv_b = A_b*xv_b + b_b*f_tot_b;
    
    % Computable part of z
    z_Ti = zPrev + 1/(2*Fs)*yPrev;
    
    v_rSum = sum(xv_r,2);
    v_rSum = v_rSum(2);
    
    v_bSum = xv_b(2);
    
    w = (rand(1)*2-1)*abs(v_bSum)*f_N;
    
    % Computable part of v with computable part of force
    v_Ti = 1/(1+sig2*bv)* ...
        ((v_bSum+bv_b*(fe_b-sig0*z_Ti-sig3*w))+...
        (-v_rSum - bv_r*(sig0*z_Ti+sig3*w)));    
    
    % Newton Rhapson
    count = 1;
    err = 99;
    while err > errMax && count < 1000
        vNew = v_Ti + K1*h0;
        zNew = z_Ti + K2*h0;
        
        % Find Zss
        Zss = (sign(vNew)/sig0)*(f_c+(f_s-f_c)*exp(-(vNew/v_s)^2));
        if vNew==0
            zss=f_s/sig0;
        end
        
        % Find alpha tilde
        if sign(zNew) ~= sign(vNew)
            aNew = 0;
        elseif abs(zNew) < Zba
            aNew = 0;
        elseif abs(zNew) > Zss
            aNew = 1;
        else
            aNew = 0.5*(1+sin(pi*((zNew-0.5*(Zss+Zba))/(Zss-Zba))));
        end
        
        % Compute g
        gNom = vNew*(1-aNew*zNew/Zss)-h0;
        
        % Compute derivatives needed for derivative of g
        % Zss/v, a/z, a/v to compute dotz/v and dotz/z
        ZssvDeri = -sign(vNew) *...
            (2*vNew)/(sig0*v_s^2) * (f_s-f_c)*exp(-(vNew/v_s)^2);
        
        if Zba < abs(zNew) && abs(zNew) < Zss && sign(vNew) == sign(zNew)
            temp = 0.5*pi * cos(pi*(zNew-0.5*(Zss+Zba))/(Zss-Zba));
            azDeri = temp * (1/(Zss-Zba));
            avDeri = temp * ((ZssvDeri*(Zba-zNew))/(Zss-Zba)^2);
        else
            azDeri = 0;
            avDeri = 0;
        end
        
        % Root-finding
        derZ = -(vNew/Zss)*(zNew*azDeri+aNew);
        derV = 1-zNew*(((aNew + vNew*avDeri)*Zss-aNew*vNew*ZssvDeri)/(Zss^2));
        gDeri = derV*K1 + derZ*K2 - 1;
        
        h1 = h0 - gNom/gDeri;
        
        % Update count and find error value
        count = count+1;
        maxIt = max(count, maxIt);
        err = abs(h1 - h0);
        
        h0 = h1;
    end
    hs(t) = count;   
    
    % Update state variables with new value for dotz
    dotz = h0;
    v = v_Ti+K1*dotz;
    z = z_Ti+K2*dotz;
    zPrev = z;
    yPrev = h0;
    
    % Compute friction force
    f_fr(t) = sig0*z + sig1*dotz + sig2*v+sig3*w; % with random component
    
    % Total force acting on bow and resonator
    f_tot_b=fe_b -f_fr(t);                  % total force on bow
    f_tot_r=f_fr(t);                  % total force on resonator
    
    % Update state variables with total force
    for j=1:modes
        xv_r(:,j) = xv_r(:,j) + b_r(:,j)*f_tot_r;
    end
    
    xv_b = xv_b + b_b*f_tot_b;
     
    % Output signal
    sig(t) = sum(xv_r(1,:));
end

soundsc(sig,Fs);

% Plots
figure(1)
subplot(3,1,1)
plot(sig)
title('Output Signal')
subplot(3,1,2)
plot(f_fr)
title('Force')

subplot(3,1,3)
plot(hs)
title('NR Iterations')
