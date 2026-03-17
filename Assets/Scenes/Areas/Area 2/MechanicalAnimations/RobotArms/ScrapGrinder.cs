using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScrapGrinder : MonoBehaviour
{
    Rigidbody2D scrapRB;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Scrap"))
        {
            scrapRB = collision.gameObject.GetComponent<Rigidbody2D>();
            StartCoroutine(Grind());
        }
    }

    public IEnumerator Grind()
    {
        scrapRB.gravityScale = 0f;
        scrapRB.linearVelocity = Vector2.zero;
        Vector2 targetPos = scrapRB.position - new Vector2(0f, 2.5f);
        Vector2 curVelocity = Vector2.zero;

        yield return new WaitForSeconds(.75f);

        while (true)
        {
            if (Mathf.Abs(scrapRB.position.y - targetPos.y) < .05f)
            {
                DestroyScrap();
                yield break;
            }
            Vector2 newPos = Vector2.SmoothDamp(scrapRB.position, targetPos, ref curVelocity, .05f, 1f, Time.fixedDeltaTime);
            scrapRB.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
            //yield return null;
        }
    }

    public void DestroyScrap()
    {
        Destroy(scrapRB.gameObject);
    }
}
