class Parser:
    def __init__(self):
        pass

    # rec_msg arg used for testing
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

            for str_obs in info_list:
                if str_obs[-1:] == " ":
                    str_obs = str_obs[:-1]

            # Remove trailing spaces
            for index, str_obs in enumerate(info_list):
                if str_obs[-1:] == " ":
                    info_list[index] = str_obs[:-1]

            # Return list of observable objects, discard first element
            return [info_list[1::]]

            # l = left, r = right, c = center
            # ((name) distance direction)
