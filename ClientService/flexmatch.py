import uuid

import boto3
from enum import Enum


class Mode(Enum):
    Casual = 'casual'
    Rank = 'rank'


class AWSFlexMatch:
    def __init__(self, region: str, aws_access_key_id: str, aws_secret_access_key: str):
        self.__region = region
        self.__aws_access_key_id = aws_access_key_id
        self.__aws_secret_access_key = aws_secret_access_key

    def create_ticket(self, mode: Mode) -> dict:
        connection = self.__get_gamelift_client()
        configuration_name = 'PongMatchmaker'
        player_data = {
            'PlayerId': str(uuid.uuid4()),
            'PlayerAttributes': {
                'mode': {'S': mode.value}
            }
        }

        # https://boto3.amazonaws.com/v1/documentation/api/latest/reference/services/gamelift.html#GameLift.Client.start_matchmaking
        response = connection.start_matchmaking(
            ConfigurationName=configuration_name,
            Players=[
                player_data,
            ]
        )

        return response['MatchmakingTicket']

    def get_ticket(self, ticket_id):
        connection = self.__get_gamelift_client()

        response = connection.describe_matchmaking(
            TicketIds=[
                ticket_id,
            ]
        )

        return response['TicketList'][0]

    def delete_ticket(self, ticket_id):
        connection = self.__get_gamelift_client()

        connection.stop_matchmaking(
            TicketId=ticket_id
        )

    def __get_gamelift_client(self):
        return boto3.client(
                'gamelift',
                region_name=self.__region,
                aws_access_key_id=self.__aws_access_key_id,
                aws_secret_access_key=self.__aws_secret_access_key
            )
