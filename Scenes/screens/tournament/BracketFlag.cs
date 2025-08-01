using Godot;

public partial class BracketFlag : TextureRect
{
    private TextureRect border;
    private Label scoreLabel;

    public override void _Ready()
    {
        border = GetNode<TextureRect>("Border");
        scoreLabel = GetNode<Label>("ScoreLabel");
    }

    public void SetAsCurrentTeam()
    {
        border.Visible = true;
    }

    public void SetAsWinner(string score)
    {
        scoreLabel.Text = score;
        scoreLabel.Visible = true;
        border.Visible = false;
    }

    public void SetAsLoser()
    {
        Modulate = new Color(0.2f, 0.2f, 0.2f, 1f);
        border.Visible = false;
    }
}