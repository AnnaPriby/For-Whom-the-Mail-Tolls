using UnityEngine;

public class DatabaseBootstrapper : MonoBehaviour
{
    [Header("Keep references alive")]
    public GreetingDatabase greeting;
    public AcknowledgementDatabase acknowledgement;
    public OpinionDatabase opinion;
    public SolutionDatabase solution;
    public GoodbyeDatabase goodbye;
    public JessicaEmailsDatabase jessicaEmails;

    public StoryEmailsDatabase storyEmails;
    public StoryAcknowledgementDatabase storyAcknowledgement;
    public StoryOpinionDatabase storyOpinion;
    public StorySolutionDatabase storySolution;

    public CoffeeEmailsDatabase CoffeeEmails;
    public CoffeeResponsesDatabase CoffeeResponses;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Optional: persist through scenes
        Debug.Log("✅ DatabaseBootstrapper holding all email databases.");
    }
}