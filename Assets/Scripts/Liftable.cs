using UnityEngine;

public class Liftable : MonoBehaviour
{
    public static Liftable LiftTarget;
    public static bool Lifted;

    public Physics Physics;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !Lifted)
        {
            LiftTarget = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (LiftTarget == this && collision.CompareTag("Player") && !Lifted)
        {
            LiftTarget = null;
        }
    }

    public static void Lift(Physics lifter)
    {
        if (LiftTarget == null || Lifted) return;

        Utilities.Instance.SpritePop(LiftTarget.GetComponent<SpriteRenderer>(), Color.white);

        Lifted = true;
        LiftTarget.transform.SetParent(lifter.transform, true);

        lifter.CombinePhysics(LiftTarget.Physics);
    }

    public static void Drop(Physics lifter)
    {
        if (LiftTarget == null || !Lifted) return;

        Lifted = false;
        LiftTarget.transform.parent = null;

        lifter.DisconnectPhysics(LiftTarget.Physics);
    }
}
