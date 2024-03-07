using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
   [SerializeField] private Player player;
   private Animator _animator;
   private const string IsWalking = "IsWalking";

   private void Awake()
   {
      _animator = GetComponent<Animator>();
   }

   private void Update()
   {
      if (!IsOwner) return;
      
      if (player != null)
         _animator.SetBool(IsWalking, player.Is_Walking());
   }
}
