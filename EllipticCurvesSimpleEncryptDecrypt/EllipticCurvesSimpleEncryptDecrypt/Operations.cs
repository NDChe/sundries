namespace Crypto5
{
    internal class Operations
    {
        private readonly int _modulus;
        private readonly int _a;
        private readonly int _b;
        public readonly List<Coordinates> GCoordinates;
        private Dictionary<char, Coordinates> _alphabet;
        private const int _o = -999;
        public Operations(int modulus, int a, int b)
        {
            _modulus = modulus;
            _a = a;
            _b = b;
            GCoordinates = GetGPointsList();
            _alphabet = GenerateAlphabet();
        }
        public Coordinates Add(Coordinates addendum1, Coordinates addendum2)
        {
            if (addendum1.IsInf)
            {
                return addendum2;
            }

            if (addendum2.IsInf)
            {
                return addendum1;
            }

            if ((addendum1.X == addendum2.X) && (addendum2.Y == -addendum2.Y))
            {
                return new Coordinates(0, 0)
                {
                    IsInf = true
                };
            }

            var alpha = GenerateAlpha(addendum1, addendum2);

            if (alpha == _o)
            {
                return new Coordinates(0, 0) { IsInf = true };
            }
            var newX = MathMod((int)Math.Pow(alpha, 2) - addendum1.X - addendum2.X);
            var newY = MathMod(alpha * (addendum1.X - newX) - addendum1.Y);

            return new Coordinates(newX, newY);
        }

        public Coordinates Subtract(Coordinates p, Coordinates q)
        {
            q.Y = MathMod(-q.Y);
            return Add(p, q);
        }

        public Coordinates Multiply(Coordinates coordinate1, int k)
        {
            var buffer = coordinate1;

            for (var i = 0; i < k - 1; ++i)
                buffer = Add(buffer, coordinate1);

            return buffer;
        }

        public string ReceiveByBob(string msg, int personalKey)
        {
            var decryptedResult = "";

            for (var i = 0; i < msg.Length; i += 2)
            {
                var firstEncryptedCoordinate = _alphabet[msg[i]];
                var secondEncryptedCoordinate = _alphabet[msg[i + 1]];
                var decryptedCoordinate = Subtract(secondEncryptedCoordinate, (Multiply(firstEncryptedCoordinate, personalKey)));
                var decryptedChar = _alphabet.FirstOrDefault(x => x.Value.Equals(decryptedCoordinate)).Key;
                decryptedResult += decryptedChar;
            }

            Console.WriteLine($"\n Decrypted message: {decryptedResult}");

            return decryptedResult;
        }

        public string SendByAlice(string msg, Coordinates publicKey, int k, Coordinates chosenG)
        {
            var result = "";

            foreach (var c in msg)
            {
                var firstEncryptedCoordinate = Multiply(chosenG, k);

                var coordinateFromAlphabet = _alphabet[c];

                var secondEncryptedCoordinate = Add(coordinateFromAlphabet, Multiply(publicKey, k));

                var firstEncryptedChar = _alphabet.FirstOrDefault(x => x.Value.Equals(firstEncryptedCoordinate)).Key;
                var secondEncryptedChar = _alphabet.FirstOrDefault(x => x.Value.Equals(secondEncryptedCoordinate)).Key;

                result += firstEncryptedChar;
                result += secondEncryptedChar;
            }

            Console.WriteLine($"\n Encrypted message: {result}");

            return result;
        }

        private List<Coordinates> GetGPointsList() // true
        {
            var points = new List<Coordinates>();

            points.Add(new Coordinates(0, 0) { IsInf = true });

            for (int x = 0; x < _modulus; ++x)
            {
                for (int y = 0; y < _modulus; ++y)
                {
                    if (((int)Math.Pow(y, 2) % _modulus) == (((int)Math.Pow(x, 3) + x + 3)) % _modulus)
                    {
                        points.Add(new Coordinates(x, y));
                    }
                }
            }

            return points;
        }

        private bool InverseModulo(int number, out int result)
        {
            int n = number;
            int m = _modulus, v = 0, d = 1;
            while (n > 0)
            {
                int t = m / n, x = n;
                n = m % x;
                m = x;
                x = d;
                d = checked(v - t * x); // Just in case
                v = x;
            }
            result = v % _modulus;
            if (result < 0) result += _modulus;
            if ((long)number * result % _modulus == 1L) return true;
            result = default;
            return false;
        }

        private static bool IsPrime(int number)
        {
            switch (number)
            {
                case <= 1:
                    return false;
                case 2:
                    return true;
            }

            if (number % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            for (var i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }

        private Dictionary<char, Coordinates> GenerateAlphabet()
        {
            var charactersList = new char[] { };

            try
            {
                charactersList = Alphabet.ReturnChars(GCoordinates.Count);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            var result = GCoordinates.Zip(charactersList, (k, v) => new { k, v })
                .ToDictionary(x => x.v, x => x.k);

            return result;
        }

        private int MathMod(int a)
        {
            while (a < 0)
            {
                a += _modulus;
            }
            return (a % _modulus);
        }

        public int GenerateAlpha(Coordinates point1, Coordinates point2)
        {
            int reversed;
            if (!point1.Equals(point2))
            {
                if (point1.X == point2.X)
                {
                    return _o;
                }
                InverseModulo(MathMod(point2.X - point1.X), out reversed);
                return MathMod((point2.Y - point1.Y) * reversed);
            }
            else
            {
                InverseModulo(MathMod(2 * point1.Y), out reversed);
                return MathMod((3 * (int)Math.Pow(point1.X, 2) + _a) * reversed);
            }

        }
    }
}
