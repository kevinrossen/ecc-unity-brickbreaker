// BAD: Brick knows about too many things!
public class Brick : MonoBehaviour
{
    void Hit()
    {
        // Brick has to find and talk to everyone directly
        FindObjectOfType<ScoreManager>().AddScore(100);
        FindObjectOfType<AudioManager>().PlaySound("brick_break");
        FindObjectOfType<ParticleManager>().SpawnParticles(transform.position);
        FindObjectOfType<UIManager>().ShowBrickDestroyed();
        FindObjectOfType<ComboSystem>().IncrementCombo();
        
        Destroy(gameObject);
    }
}