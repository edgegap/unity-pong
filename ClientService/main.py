import os
import typing

from flask import Flask, request

from flexmatch import AWSFlexMatch, Mode

app = Flask(__name__)

matchmaker: typing.Optional[AWSFlexMatch] = None


@app.route("/v1/tickets", methods=['POST'])
def create_ticket():
    post_data = request.get_json()
    ticket = matchmaker.create_ticket(Mode(post_data['mode']))

    print(f'Created ticket {ticket["TicketId"]}')

    return {
        'data': {
            'ticket_id': ticket['TicketId']
        }
    }


@app.route("/v1/tickets/<ticket_id>", methods=['GET'])
def get_ticket(ticket_id: str):
    ticket = matchmaker.get_ticket(ticket_id)

    print(f'Fetched ticket {ticket_id}')

    assignment = None

    if 'GameSessionConnectionInfo' in ticket:
        info = ticket['GameSessionConnectionInfo']
        assignment = {
            'server_host': f"{info['IpAddress']}:{info['Port']}"
        }

    return {
        'data': {
            'ticket_id': ticket['TicketId'],
            'assignment': assignment
        },
    }


@app.route("/v1/tickets/<ticket_id>", methods=['DELETE'])
def delete_ticket(ticket_id: str):
    matchmaker.delete_ticket(ticket_id)

    print(f'Deleted ticket {ticket_id}')

    return {
        'data': {
            'id': ticket_id
        }
    }


def get_env_or_fail(key: str):
    result = os.environ.get(key)

    if result is None:
        raise KeyError(f'Could not find {key}')

    return result


if __name__ == '__main__':
    region = get_env_or_fail('AWS_REGION')
    aws_access_key_id = get_env_or_fail('AWS_KEY_ID')
    aws_secret_access_key = get_env_or_fail('AWS_SECRET')

    matchmaker = AWSFlexMatch(
        region=region,
        aws_access_key_id=aws_access_key_id,
        aws_secret_access_key=aws_secret_access_key,
    )

    app.run(host='0.0.0.0', port=8001)
