function [b0, b1, a0] = ExpDecOnePole3(dampDeg, len, fs)
    D = zeros(len,3);

    for j = 1:3
        damping = dampDeg(j)/fs;
        for i=1:len
            D(i,j) = exp(-damping*(i-1));
        end
    end

    b0 = D(:,1);
    b1 = D(:,2);
    a0 = D(:,3)- 1;
end

