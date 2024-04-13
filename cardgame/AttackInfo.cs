public class AttackInfo
{
    public Card target;
    public FightClub fightManager;

    public AttackInfo(Card target, FightClub fightManager)
    {
        this.target = target;
        this.fightManager = fightManager;
    }
}
