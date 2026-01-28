using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    [Header("Game Elements")]
    [Range(2, 6)]
    [SerializeField] private int difficulty = 4;
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;


    [Header("UI Elements")]
    [SerializeField] private List<Texture2D> imageTextures;
    [SerializeField] private Transform levelSelectPanel;
    [SerializeField] private Image levelSelectPrefab;

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;

    void Start()
    {
        //create the UI
        foreach (Texture2D texture in imageTextures)
        {
            Image image = Instantiate(levelSelectPrefab, levelSelectPanel);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            //assign button action
            image.GetComponent<Button>().onClick.AddListener(delegate { StartGame(texture); });
        }
    }



    public void StartGame(Texture2D jigsawTexture)
    {
        //hide the UI
        levelSelectPanel.gameObject.SetActive(false);
        //store a list of the transform for each piece
        pieces = new List<Transform>();
        //calculate dimensions based on difficulty
        dimensions = GetDimensions(jigsawTexture, difficulty);
        //create pieces
        CreateJigsawPieces(jigsawTexture);
    }



    Vector2Int GetDimensions(Texture2D jigsawTexture, int difficulty)
    {
        Vector2Int dimensions = Vector2Int.zero;
        if (jigsawTexture.width < jigsawTexture.height)
        {
            dimensions.x = difficulty;
            dimensions.y = (difficulty * jigsawTexture.height) / jigsawTexture.width;
        }
        else
        {
            dimensions.x = (difficulty * jigsawTexture.width) / jigsawTexture.height;
            dimensions.y = difficulty;
        }

        return dimensions;
    }



    void CreateJigsawPieces(Texture2D jigsawTexture)
    {
        //calculate piece size
        height = 1f / dimensions.y;
        float aspect = (float)jigsawTexture.width / jigsawTexture.height;
        width = aspect / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                //create piece
                Transform piece = Instantiate(piecePrefab, gameHolder);
                piece.localPosition = new Vector3(
                    (-width * dimensions.x / 2) + (width * col) + (width / 2),
                    (-height * dimensions.y / 2) + (height * row) + (height / 2),
                    -1);
                piece.localScale = new Vector3(width, height, 1f);
                piece.name = $"Piece {(row * dimensions.x) + col}";
                pieces.Add(piece);
                //set piece texture
                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;
                //calculate UVs
                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(col * width1, row * height1);
                uv[1] = new Vector2((col + 1) * width1, row * height1);
                uv[2] = new Vector2(col * width1, (row + 1) * height1);
                uv[3] = new Vector2((col + 1) * width1, (row + 1) * height1);
                //apply UVs to mesh
                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                mesh.uv = uv;
                //set piece correct position
                piece.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jigsawTexture);
            }
        }
    }
}