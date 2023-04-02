using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class SubmitText : MonoBehaviour
{
    public TMP_InputField inputField;
    public string textFinal;

    public TextMeshProUGUI enterMovieSynopsis;
    public TextMeshProUGUI rating, timeLeft;
    public GameObject submit, border, tryAgain;

    public GameObject popcornSplosion, popcornSplosionSlow;

    public float time;
    public TextMeshProUGUI timer;
    public TimeSpan ts = TimeSpan.FromSeconds(90);
    public bool timerHit0, buttonClicked;

    public Vector2 submitinitpos;

    public AudioSource buttonClick, ratingBong, spinnerClick;

    // Start is called before the first frame update
    void Start()
    {
        tryAgain.active = false;
        rating.enabled = false;
        submit = GameObject.Find("Submit");
    }

    // Update is called once per frame
    void Update()
    {
        //once timer reaches 0 stop counting down and call submitText method
        if (timer.text != "Time Left: 00:00")
        {
            ts = ts.Subtract(TimeSpan.FromSeconds(Time.deltaTime));
            timer.text = "Time Left: " + ts.ToString(@"mm\:ss");
        }
        else if(!timerHit0 && !buttonClicked)
        {
            Debug.Log("timer didnt hit 0 and button wasnt clicked");
            timerHit0 = true;
            submitText();
        }
    }

    //when submit button is clicked or the time runs out, call this method
    public void submitText()
    {
        textFinal = inputField.text;
        buttonClicked = true;

        //sound effects and particle systems
        buttonClick.Play();
        Instantiate(popcornSplosion, submitinitpos, Quaternion.identity);
        
        
        Debug.Log("Text submitted:\n" + textFinal);
        //turns on and off the right UI elements after the submit button is clicked
        enterMovieSynopsis.enabled = false;
        timeLeft.enabled = false;
        //submit.active = false; this breaks the coroutine so just move it away instead
        submit.transform.position = new Vector2(-400, -400);
        border.active = false;
        inputField.enabled = false;
        inputField.interactable = false;
        tryAgain.active = true;
        rating.enabled = true;

        //if text is empty then just return 0 (with cool animation)
        if(textFinal == "")
        {
            StartCoroutine(randomizeRatingAnim());
        }
        else
        {
            //submit text to AI algorithm and return rating based off of that
            StartCoroutine(GetRequest(string.Format("http://75.40.154.54:2023/rate?overview={0}", textFinal)));
        }
    }

    //restarts scene
    public void restart()
    {
        SceneManager.LoadScene("Main");
    }

    //gets webrequest
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            //error casees with webrequest
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    //randomize number output
                    for (int i = 0; i < 5; i++)
                    {
                        rating.text = "Rating: " + UnityEngine.Random.RandomRange(0, 100)*.1;
                        spinnerClick.Play();
                        yield return new WaitForSeconds(.05f);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        rating.text = "Rating: " + UnityEngine.Random.RandomRange(0, 100) * .1;
                        spinnerClick.Play();
                        yield return new WaitForSeconds(.1f);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        rating.text = "Rating: " + UnityEngine.Random.RandomRange(0, 100) * .1;
                        spinnerClick.Play();
                        yield return new WaitForSeconds(.2f);
                    }

                    //sets rating text to webrequest number
                    rating.text = "Rating: " + webRequest.downloadHandler.text;
                    ratingBong.Play();
                    break;
            }
        }
    }

    //randomize rating animation for when box is empty
    IEnumerator randomizeRatingAnim()
    {
        for(int i = 0; i < 5; i++)
        {
            rating.text = "Rating: " + UnityEngine.Random.RandomRange(0, 100) * .1;
            spinnerClick.Play();
            yield return new WaitForSeconds(.05f);
        }
        for (int i = 0; i < 4; i++)
        {
            rating.text = "Rating: " + UnityEngine.Random.RandomRange(0, 100) * .1;
            spinnerClick.Play();
            yield return new WaitForSeconds(.1f);
        }
        for (int i = 0; i < 3; i++)
        {
            rating.text = "Rating: " + UnityEngine.Random.RandomRange(0, 100) * .1;
            spinnerClick.Play();
            yield return new WaitForSeconds(.2f);
        }
        ratingBong.Play();
        rating.text = "Rating: 0.0";
    }

    //quit method
    public void quitApp()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
