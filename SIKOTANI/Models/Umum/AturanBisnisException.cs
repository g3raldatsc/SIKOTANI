namespace SIKOTANI.Models;

/// <summary>Dilempar bila sebuah aturan bisnis dilanggar.</summary>
public class AturanBisnisException : Exception
{
    public AturanBisnisException(string pesan) : base(pesan) { }
}