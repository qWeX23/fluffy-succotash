import discord
import asyncio
import json

#Load connections config
with open("connections.json",'r') as file:
   connections =  json.loads(file.read())

client = discord.Client()
event_loop = client.loop

@client.event
async def on_ready():
    #Create internal listener from connections config
    event_loop.create_task(asyncio.start_server(soc_handle, connections['internal_ip'],connections['internal_port'], loop=event_loop))
    #Alive
    print('Logged in as {} working on {}'.format(client.user.name,list(client.servers)[0].name))
    print('socket listening on {}:{}'.format( connections['internal_ip'],connections['internal_port']))


async def soc_handle(reader, writer):
   
    print('internal listener triggered')
    message = await reader.read(1000000)
    message = json.loads(message)
    addr = writer.get_extra_info('peername')
    print(addr)
    if(message['function'] == 'minute_data'):
        writer.write(get_data())
        await writer.drain()
        writer.close()

def get_data():
    mems = []
    for mem in list(list(client.servers)[0].members):
        obj={
            'discord_id' : mem.id,
            'discord_name' :mem.name,
            'discriminator':mem.discriminator,
            'game' : mem.game.name if mem.game is not None else 'No Game',
            'status' : str(mem.status)
        }
        mems.append(obj)
    p = json.dumps(mems)
    return p.encode()

client.run(connections['client_id'])