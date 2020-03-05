class Parser:
    def __init__(self):
        self.yeet = 0

    # rec_msg arg used for testing
    # TODO: can only be used after kick_off
    def parse_info(self, rec_msg, player):
        # Remove all ) as they clash when extracting last number of object
        rec_msg = rec_msg.replace(')', '')
        # Get the first 5 chars, used to recognize the type of msg
        msg_type = rec_msg[:5]

        if msg_type == "(sens":
            # In order to only get the required numbers, we spilt the string by spaces and load it into a list
            info_list = rec_msg.rsplit(" ")
            # the 6th element is current stamina
            player.stamina = int(info_list[6])
            player.speed = float(info_list[10])
            player.effort = float(info_list[7])


        elif msg_type == "(hear":
            # referee change mode OR player say
            info_list = rec_msg.rsplit(" ")
            if info_list[2] == "referee":
                player.game_status = info_list[3]

        elif msg_type == "(init":
            # player initialized
            print("Player connected")

        elif msg_type == "(see ":
            # Split at (( to get every object and its info separated
            info_list = rec_msg.rsplit("((")
            observables = [info_list[1::]]
            
            # l = left, r = right, c = center
            # ((name) distance direction)
            pass
