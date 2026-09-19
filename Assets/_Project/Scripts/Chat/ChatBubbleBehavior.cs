using System;
using System.Collections;
using Shared.Model.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ChatBubbleBehavior : MonoBehaviour
{
    private enum Alignment
    {
        Left, 
        Right
    }

    public DateTime Timestamp;
        
    private Alignment _alignment;
    private bool _lastIsSelfAuthored;
    public bool IsSelfAuthored;
    private UserType _lastIsFromUserType;
    public UserType IsFromUserType;
    private bool _needsUpdate;
    private ScrollRect _parentScrollRect => transform.GetComponentInParent<ScrollRect>();
    private VerticalLayoutGroup _parentLayoutGroup => transform.GetComponentInParent<VerticalLayoutGroup>();
    private RectTransform _selfRect => transform.GetComponent<RectTransform>();
    private RectTransform _outerContainerRect => transform.Find( "OuterContainer" ).GetComponent<RectTransform>();
    private RectTransform _innerContainerRect => transform.Find( "OuterContainer/InnerContainer" ).GetComponent<RectTransform>();
    private RectTransform _headerRect => _outerContainerRect.Find( "InnerContainer/Header" ).GetComponent<RectTransform>();
    private TextMeshProUGUI _headerText =>  _headerRect.GetComponentInChildren<TextMeshProUGUI>();
    private RectTransform _contentRect => _outerContainerRect.Find( "InnerContainer/Content" ).GetComponent<RectTransform>();
    private TextMeshProUGUI _contentText => _contentRect.GetComponentInChildren<TextMeshProUGUI>();
    private Image _contentImage => _contentRect.GetComponent<Image>();

    void Start()
    {
        _alignment = IsSelfAuthored ? Alignment.Right : Alignment.Left;
        _lastIsSelfAuthored = IsSelfAuthored;
        SetAlignment( _alignment );
    }

    public void SetMessage(DateTime sentAt, string username, string message, bool isSelfAuthored)
    {
        var headerText = $"{sentAt:yyyy-MM-dd HH:mm} | {username}";
        _headerText.text = headerText;
        _contentText.text = message;
        IsSelfAuthored = isSelfAuthored;
        UpdatePosition();
        _needsUpdate = true;
    }

    private void UpdatePosition()
    {
        if( transform.parent.childCount <= 1 )
            return;
            
        transform.SetAsLastSibling();
        if( transform.parent.GetChild( transform.parent.childCount - 1 ).GetComponent<ChatBubbleBehavior>().Timestamp < Timestamp )
        {
            // This is the newest message, leave it where it is.
            return;
        }

        for( var i = transform.parent.childCount - 1; i >= 0; i-- )
        {
            // Loop until we find an older message
            if( transform.parent.GetChild( i ).GetComponent<ChatBubbleBehavior>().Timestamp < Timestamp )
            {
                transform.SetSiblingIndex( i + 1 );
                break;
            }
        }
    }

    private void Update()
    {
        if( _lastIsSelfAuthored != IsSelfAuthored )
        {
            SwapAlignment();
            _needsUpdate = true;
        }
        if( _lastIsFromUserType != IsFromUserType )
        {
            _lastIsFromUserType = IsFromUserType;
            _needsUpdate = true;
        }

        if( _needsUpdate )
        {
            StartCoroutine( UpdateRectSizes() );
        }
    }

    private IEnumerator UpdateRectSizes()
    {
        _needsUpdate = false;
            
        _contentText.ForceMeshUpdate();
        yield return null;
        var preferredHeaderValues =_headerText.GetPreferredValues( _headerText.text );
        var totalHeaderWidth = preferredHeaderValues.x - _headerText.rectTransform.offsetMax.x + _headerText.rectTransform.offsetMin.x;
        _headerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalHeaderWidth );
        _headerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, _headerRect.rect.height );

        var maxWidth = _selfRect.rect.width - 150;
        var preferredContentValues =_contentText.GetPreferredValues( _contentText.text, maxWidth, 0 );

        if( preferredContentValues.x >= maxWidth )
        {
            _outerContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
            _contentText.ForceMeshUpdate();

            var renderedValues = _contentText.GetRenderedValues();
            _outerContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, renderedValues.y + 95 - preferredContentValues.y);
            _selfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, renderedValues.y + 95 - preferredContentValues.y);
        }
        else
        {
            var preferredContentWidth = preferredContentValues.x + 
                _contentText.rectTransform.offsetMin.x - _contentText.rectTransform.offsetMax.x +
                _innerContainerRect.offsetMin.x - _innerContainerRect.offsetMax.x;
            var totalHeaderWidthIncMargins = totalHeaderWidth +
                _contentText.rectTransform.offsetMin.x - _contentText.rectTransform.offsetMax.x;
            var totalContentWidth = Math.Max( totalHeaderWidthIncMargins, preferredContentWidth );
                
            _outerContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalContentWidth );
            _contentText.ForceMeshUpdate();

            var renderedValues = _contentText.GetRenderedValues();
            _outerContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, renderedValues.y + 45 );
            _selfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, renderedValues.y + 45 );
        }

        yield return null;
        if( _parentLayoutGroup.spacing > 0.01f )
        {
            _parentLayoutGroup.spacing = 0f;
        }
        else
        {
            _parentLayoutGroup.spacing = 0.02f;
        }

        yield return null;
        _parentScrollRect.verticalNormalizedPosition = 0;
    }

    private void SwapAlignment()
    {
        SetAlignment( _alignment == Alignment.Left ? Alignment.Right : Alignment.Left );
        _lastIsSelfAuthored = IsSelfAuthored;
    }

    private void SetAlignment( Alignment alignment )
    {
        if( alignment == Alignment.Left )
        {
            _outerContainerRect.pivot = Vector2.up;
            _outerContainerRect.anchorMin = Vector2.up;
            _outerContainerRect.anchorMax = Vector2.up;
            _outerContainerRect.anchoredPosition = Vector2.right * 10;
                
            _headerRect.pivot = Vector2.up;
            _headerRect.anchorMin = Vector2.up;
            _headerRect.anchorMax = Vector2.up;
            _headerRect.anchoredPosition = Vector2.down * 10;

            _headerText.alignment = TextAlignmentOptions.Left;

            switch( IsFromUserType )
            {
                case UserType.Standard:
                    _contentImage.color = new Color( 1f, 0.9825011f, 0.9294118f );
                    break;
                case UserType.System:
                    _contentImage.color = new Color( 0.8962264f, 0.8425564f, 0.4777056f );
                    break;
                case UserType.Moderator:
                    _contentImage.color = new Color( 0.495372f, 0.5494857f, 0.8679245f );
                    break;
            }
        }
        else
        {
            _outerContainerRect.pivot = Vector2.one;
            _outerContainerRect.anchorMin = Vector2.one;
            _outerContainerRect.anchorMax = Vector2.one;
            _outerContainerRect.anchoredPosition = Vector2.left * 10;
                
            _headerRect.pivot = Vector2.one;
            _headerRect.anchorMin = Vector2.one;
            _headerRect.anchorMax = Vector2.one;
            _headerRect.anchoredPosition = Vector2.down * 10;

            _headerText.alignment = TextAlignmentOptions.Right;

            _contentImage.color = new Color(0.759434f, 0.8501809f, 1);
        }

        _alignment = alignment;
        _needsUpdate = true;
    }
}