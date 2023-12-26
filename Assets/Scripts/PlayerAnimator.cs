using UnityEngine;

public class PlayerAnimator : MonoBehaviour
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
      if (player != null)
      {
         _animator.SetBool(IsWalking, player.Is_Walking());
      }
   }
}
