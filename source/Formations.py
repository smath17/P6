# Contains dictionaries for different formations based on the player's uniform number
# Needed as multiple objects are run simultaneously


def standard_formation(unum):
    return {
        1: [-49, 0],
        2: [-25, -5],
        3: [-25, 5],
        4: [-24, -18],
        5: [-24, 18],
        6: [-18, 0.5],
        7: [-13, -11],
        8: [-13, 11],
        9: [-1, -17],
        10: [-1, 17],
        11: [-10, 0]
    }[unum]


def g_formation(unum):
    return {
        1: [-50, 0],
        2: [-20, -8],
        3: [-20, 8],
        4: [-18, -18],
        5: [-18, 18],
        6: [-15, 0],
        7: [0, -12],
        8: [0, 12],
        9: [10, -22],
        10: [10, 22],
        11: [10, 0]
    }[unum]


# wrapper for g letter formation, so letter always faces correct way
def g_letter_wrapper(unum, side):
    xy_set = g_formation(unum)
    if not side == 'l':
        mirror_left_side(xy_set)
    return xy_set


# Changes the position to mirror that of left side, as right sides' y-coords are flipped
def mirror_left_side(xy_set):
    xy_set[1] = -xy_set[1]
    return xy_set


# Shift letter to the side, depending on which side player is
def shift_letter(xy_set, side):
    if side == 'l':
        xy_set[0] += 10
    elif side == 'r':
        xy_set[0] -= 10
    return xy_set
