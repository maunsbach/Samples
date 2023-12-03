function [s] = digitalWaveguideS2(fs, zBS, b0, b1, a0, C, d_s, lambda, e)

ys = zeros(fs+zBS,length(d_s));
s = zeros(fs+zBS,1);

for n=1+zBS:zBS+fs
    s(n) = b0(n)*(e(n) + nestedE(n)) + b1(n)*(e(n - 1) + nestedE(n-1)) - a0(n) * s(n-1);
    %s(n) = b0(n)*(e(n) + nestedE(n));
end

    function e_f = nestedE(n0)
        e_f = 0;
        for i=1:length(lambda)
           e_temp = lambda(i)*(C(i, 1)*s(n0-d_s(i)) + C(i, 2)*s(n0-d_s(i)-1) + C(i, 3)*s(n0-d_s(i)-2)) - C(i,4)*ys(n0-1,i) - C(i,5) * ys(n0-2,i);
           ys(n0,i) = e_temp;
           e_f = e_f + e_temp;
        end
        %sum(ys(101:end,:),2)
    end
end

