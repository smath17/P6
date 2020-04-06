import os
import unittest

from CoachController import Coach
from PlayerController import Player


class MyTestCase(unittest.TestCase):

    @classmethod
    def setUpClass(cls):
        # Change working dir to source
        path = os.getcwd()
        new_path = path[:-6]
        os.chdir(new_path)

    # Create player, connect to server and receive update
    def test_player_connect(self):
        self.player = Player("Test")
        # loop through initial server info
        for x in range(25):
            self.player.update_info()
        assert self.player.stamina == 8000
        assert self.player.game_status == "before_kick_off", "was: {}".format(self.player.game_status)

        self.player.disconnect()
        self.player.stop_connection()

    def test_goalie_connect(self):
        self.goalie = Player("Test", True)
        assert self.goalie.stamina == 0
        assert self.goalie.serverAddressPort[1] != 6000

        self.goalie.disconnect()
        self.goalie.stop_connection()

    # Create online coach
    def test_coach_connect(self):
        testmsg = '(ok look'
        player = Player("Test")
        self.coach = Coach("Test")

        # Catch up the init messages
        #for x in range(20):
            #self.coach.rec_msg()
        recmsg = self.coach.send_action("(look)")[0:8]
        assert recmsg == testmsg, "Coach received: {}, but expected: {}".format(recmsg, testmsg)

        self.coach.disconnect()
        self.coach.stop_connection()
        player.disconnect()
        player.stop_connection()


if __name__ == '__main__':
    unittest.main()
