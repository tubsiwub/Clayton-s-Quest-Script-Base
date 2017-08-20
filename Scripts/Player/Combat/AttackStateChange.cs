using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStateChange : StateMachineBehaviour
{
	[SerializeField] PlayerAttack.AttackState state;

	PlayerAttack playerAttack;

	void Awake()
	{
		playerAttack = GameObject.FindWithTag("Player").GetComponent<PlayerAttack>();
	}

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		playerAttack.SetAttackState(state);
	}
}
