import unittest

from ServerParser import Parser
from PlayerController import Player


class MyTestCase(unittest.TestCase):

    def setUp(self):
        self.parser = Parser()
        self.player = Player("TestTeam", False, False)

    def test_parse_sense(self):
        self.test_sense = "(sense_body 0 (view_mode high normal) (stamina 8000 1 130600) (speed 0 0) (head_angle 0) " \
                          "(kick 0) (dash 0) (turn 0) (say 0) (turn_neck 0) (catch 0) (move 0) (change_view 0) " \
                          "(arm (movable 0) (expires 0) (target 0 0) (count 0)) (focus (target none) (count 0)) " \
                          "(tackle (expires 0) (count 0)) (collision none) (foul  (charged 0) (card none)))"
        self.parser.parse_info(self.test_sense, self.player)

        self.test_sense = self.test_sense.replace(')', '')
        self.test_sense = self.test_sense.rsplit(" ")

        assert self.player.stamina == 8000, "Stamina is wrong, extracted int from [6], but [6] was: {}".format(
            self.test_sense[6])
        assert self.player.speed == 0, "Speed is wrong, extracted float from [10], but [10] was: {}".format(
            self.test_sense[10])
        assert self.player.effort == 1, "Effort is wrong, extracted float from [7], but [7] was: {}".format(
            self.test_sense[7])

    def test_parse_see(self):
        self.test_see = '(see 885 ((f c) 9.5 0 -0 -0) ((f c b) 43.4 4) ((f b 0) 48.4 4) ((f b l 10) 49.4 16)' \
                        ' ((f b l 40) 62.2 44) ((b) 10 0 -0 -0) ((p "Simon" 10) 30 43 -0 0 -85 -85) ' \
                        '((p "Simon") 36.6 38) ((l b) 43.8 -85))'

        self.rec_obs = self.parser.parse_info(self.test_see, self.player)[0]  # nested list

        self.correct_obs = ['f c 9.5 0 -0 -0', 'f c b 43.4 4', 'f b 0 48.4 4', 'f b l 10 49.4 16',
                            'f b l 40 62.2 44', 'b 10 0 -0 -0', 'p "Simon" 10 30 43 -0 0 -85 -85', 'p "Simon" 36.6 38',
                            'l b 43.8 -85']

        assert self.rec_obs == self.correct_obs, "List of observables not correct, got:\n" + str(self.rec_obs)

    def test_parse_hear_referee(self):
        self.test_hear_ref = "(hear 0 referee kick_off_l)"
        self.parser.parse_info(self.test_hear_ref, self.player)

        assert self.player.game_status == "kick_off_l"

    def test_parse_init_info(self):
        self.test_init_msg = "(init l 2 before_kick_off)"
        self.parser.init_info(self.player, self.test_init_msg)

        assert self.player.game_status == "before_kick_off", "Game status was = {}".format(self.player.game_status)
        assert self.player.unum == 2, "Unum was = {}".format(self.player.unum)
        assert self.player.side == 'l', "Side was = {}".format(self.player.side)


if __name__ == '__main__':
    unittest.main()
