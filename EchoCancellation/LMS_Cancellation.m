function [y, s] = LMS_Cancellation(x,u, p, mu)
% The length of the output is matched to the length of what is to be
% removed (karensVoice)
N = length(u);

% Filter coefficients
h = zeros(p,1);
% Initialize output array
y = x;
s = zeros(N,1);

% p-order requires the first p values to calculate the next
% Therefore start at p
for n=p:N
    % extract segment from x equal to p-order
    X = u(1+n-p:n);

    s(n)= h'*X;
    % Calculate the error (and output)
    % the mixed value subtracted the filtered value
    y(n) = x(n) - s(n);

    % calculate new filter coefficients
    % C = C + 2*mu*e(n)*x(n)
    h = h + 2*mu*y(n)*X;
end

end