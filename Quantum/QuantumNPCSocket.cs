using UnityEngine;
using static BandTogether.Quantum.QuantumTarget;

namespace BandTogether.Quantum;

public class QuantumNPCSocket : QuantumSocket
{
	[SerializeField] public QuantumTarget targetType = Start;
}