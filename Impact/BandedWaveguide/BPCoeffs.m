function [Cs] = BPCoeffs(fc, B)
    fc = fc/(48000);
    Q = B/fc;
    r = sin(2*pi*fc)/(2*Q);

    c0 = r/(1+r);
    c1 = 0;
    c2 = -r/(1+r);
    c3=(-2*cos(2*pi*fc))/(1+r);
    c4=(1-r)/(1+r);
    Cs = [c0 c1 c2 c3 c4];
end

