from PlayerController import Player
from KeyboardControls import KeyboardControl

if __name__ == "__main__":

    teamname1 = "Bobbers"
    controller = KeyboardControl()

    # Create a list of players
    team1 = [Player(teamname1) for i in range(11)]

    # Initially move all players from team1 onto the field
    y = -30
    for player in team1:
        player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
        y = y + 5

    controller.simple_auto_score(team1)
    controller.keymap(team1)
