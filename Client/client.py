#!/usr/bin/env python

import asyncore, socket, json

HOST="localhost"
PORT=7701
BUFFER_SIZE=0xff

class UniTestClient(asyncore.dispatcher):
    def __init__(self, host, port, message_type, body):
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.connect( (host, port) )
        bodyBuffer = json.dumps(body);
        self.buffer = 'Length: '+str(len(bodyBuffer))+'\n'+'MessageType: '+message_type+'\n\n'+bodyBuffer

    def handle_connect(self):
        pass

    def handle_close(self):
        self.close()

    def handle_read(self):
        print self.recv(BUFFER_SIZE)

    def writable(self):
        return (len(self.buffer) > 0)

    def handle_write(self):
        sent = self.send(self.buffer)
        self.buffer = self.buffer[sent:]


client = UniTestClient(HOST,PORT,"STDIN",{"func":"RunTestOfType","args":"System.Type"})
asyncore.loop()
