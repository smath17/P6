from PlayerController import Player
from CoachController import Coach


if __name__ == "__main__":

    teamname1 = "Simon"
    # Create a list of players
    team1 = [Player(teamname1, True)]
    for x in range(10):
        team1.append(Player(teamname1))
    # Create coach
    coach = Coach(teamname1)

    # Initially move all players from team1 onto the field
    y = -30

    for player in team1:
        player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
        y = y + 5

    while True:
        team1[0].update_info()
