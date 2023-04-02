using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GenreRandomizer : MonoBehaviour
{
    public TextMeshProUGUI genreText;
    public AudioSource click, bong;
    private List<string> genres = new List<string>();
    private int randomInt;
    private string genreFin;

    // Start is called before the first frame update
    void Start()
    {
        //list of genres
        AddToList("Horror", "Thriller", "Drama", "Adventure", "Documentary", "Narrative", "Music", "Mystery", "Fantasy", "Television", "Action", "Science Fiction", "Western", "Comedy", "Romantic Comedy", "Animation", "Dark Comedy", "Epic", "Slasher", "Drama", "Romance", "Crime Film", "Fantasy", "Musical", "Noir", "History");
        StartCoroutine(genreRandomizer());
    }

    // Update is called once per frame
    void Update()
    {
        //picks random int for index
        randomInt = Random.Range(0, genres.Count-1);
    }

    //method for adding multiple entries to list at once
    public void AddToList(params string[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            genres.Add(list[i]);
        }
    }

    //coroutine that randomizes genres
    public IEnumerator genreRandomizer()
    {
        //cool little animation for random genre
        for(int i = 0; i < 5; i++)
        {
            genreText.text = "Genre: " + genres[randomInt];
            click.Play();
            yield return new WaitForSeconds(.05f);
        }
        for(int i = 0; i < 4; i++)
        {
            genreText.text = "Genre: " + genres[randomInt];
            click.Play();
            yield return new WaitForSeconds(.1f);
        }
        for (int i = 0; i < 3; i++)
        {
            genreText.text = "Genre: " + genres[randomInt];
            click.Play();
            yield return new WaitForSeconds(.2f);
        }

        //final genre picked
        bong.Play();
        genreText.text = "Genre: " + genres[randomInt];
        Debug.Log("Final genre picked");
    }
}
