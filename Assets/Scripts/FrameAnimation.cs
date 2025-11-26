using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FrameAnimation : MonoBehaviour
{
    public GameObject frame;
    public GameObject jessica; 
    public GameObject koJessica;

    public GameObject deskWithDog;

    private RectTransform deskRT;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        deskRT = deskWithDog.GetComponent<RectTransform>();
        
        jessica.gameObject.SetActive(false);
        StartCoroutine(DogJumpscare());
        

    }

    IEnumerator DogJumpscare()
    {
        deskWithDog.SetActive(true);
        Vector3 deskStart = deskRT.anchoredPosition;
        deskRT.anchoredPosition += new Vector2(0, -800);
        frame.transform.DOScale(2f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        frame.gameObject.SetActive(false);
        koJessica.gameObject.SetActive(true);
        deskRT.DOAnchorPos(deskStart, 5f).SetEase(Ease.InOutSine);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
