
using Crypto5;

var calculator = new Operations(13, 3, 12);
var gCoordinates = calculator.GCoordinates;

Console.WriteLine(" Choose a G-point from the following: ");

for (var i = 0; i < gCoordinates.Count; i++)
{
    Console.Write($"{gCoordinates[i].X,2} ; {gCoordinates[i].Y,2} - №{i + 1}\n");
}

var n = Console.ReadLine();

Coordinates chosenG;

try
{
    chosenG = gCoordinates[Convert.ToInt32(n) - 1];
}
catch
{
    Console.BackgroundColor = ConsoleColor.DarkRed;
    throw new Exception("The chosen element is not present in the G points list. ");
}

Console.Clear();

Console.Write(" Input Bob's private key: ");

var bPrivateKey = Convert.ToInt32(Console.ReadLine());

var bPublicKey = calculator.Multiply(chosenG, bPrivateKey);

Console.Write($"\n\n Bob's public key is: {bPublicKey.X,3} ; {bPublicKey.Y,3}");

Console.Write($"\n\n Input message: ");

var msg = Console.ReadLine();

if (msg is null)
    return;

var emsg = calculator.SendByAlice(msg, bPublicKey, 7, chosenG);

calculator.ReceiveByBob(emsg, bPrivateKey);

Console.ReadKey();


