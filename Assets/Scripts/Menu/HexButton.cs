using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class HexButton : MonoBehaviour
{
    [SerializeField] private AudioClip Selected;
    [SerializeField] private AudioClip Pressed;

    [SerializeField] private UnityEvent OnClick;
    [SerializeField] private float MaxPosition;
    [SerializeField] private float MinPosition;

    [SerializeField] private float AnimationSpeedUp=1;
    [SerializeField] private float AnimationSpeedDown=1;

    [SerializeField] private AudioSource AudioSource;
    private bool _IsMouseOver=false;
    private HexGrid _grid;
    private Vector3 _cell;

    private void Awake() { _grid = FindObjectOfType<HexGrid>(); }
    private void Start()
    {
        //  Destroy(_grid.hexCells[_grid.InLocalCoords(transform.position).x, _grid.InLocalCoords(transform.position).y].gameObject);
        _cell = _grid.hexCells[_grid.InLocalCoords(transform.position).x, _grid.InLocalCoords(transform.position).y].transform.position;
        transform.position= new Vector3(_cell.x,MinPosition,_cell.z);
    }
    private void OnMouseDown()
    {
        OnClick.Invoke();
        AudioSource.PlayOneShot(Pressed);
    }
    private void OnMouseOver()
    {
        if(transform.position.y<= MaxPosition) transform.position += -Vector3.down * Time.fixedDeltaTime * AnimationSpeedUp;
    }

    private void OnMouseEnter() 
    {
        _IsMouseOver = true;
        AudioSource.PlayOneShot(Selected);
    }
    private void OnMouseExit() { _IsMouseOver = false; }
    
    private void FixedUpdate()
    {
        if (transform.position.y >= MinPosition && _IsMouseOver==false) transform.position += Vector3.down * Time.fixedDeltaTime * AnimationSpeedDown;
    }
}
