#!/usr/bin/env python

import asyncore, socket, json, sys

HOST="localhost"
PORT=7701
BUFFER_SIZE=0xff

COLORMAP={
        '<color=red>'       : '\033[91m',
        '<color=green>'     : '\033[92m',
        '<color=yellow>'    : '\033[93m',
        '</color>'          : '\033[0m'
         }

class UniTestClient(asyncore.dispatcher):

    recvBuffer=None
    response=None

    class Response():
        length=-1
        messageType=None
        isReadingBody=False
        isReadingBodyComplete=False
        body=None

    def __init__(self, host, port, message_type, body):
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.connect( (host, port) )
        self.request(message_type, body)

    def handle_connect(self):
        pass

    def handle_close(self):
        print "closing"
        self.close()

    def handle_read(self):
        self.consume_receive_packet(self.recv(BUFFER_SIZE))

    def writable(self):
        return (len(self.buffer) > 0)

    def handle_write(self):
        sent = self.send(self.buffer)
        self.buffer = self.buffer[sent:]

    def request(self,message_type, body):
        bodyBuffer = json.dumps(body);
        self.buffer = 'Length: '+str(len(bodyBuffer))+'\n'+'MessageType: '+message_type+'\n\n'+bodyBuffer

    def consume_receive_packet(self,received_buffer):

        if(self.recvBuffer is None):
            #print "Verbose> replacing receivedBuffer"
            self.recvBuffer = received_buffer
        else:
            #print "Verbose> adding"
            self.recvBuffer += received_buffer
        
        while(self.recvBuffer is not None and len(self.recvBuffer)>0):
            index = self.recvBuffer.index('\n')+1
            if(index > 0):
                self.process_receive_line(self.recvBuffer[:index])
                
                if(self.recvBuffer is not None):
                    self.recvBuffer = self.recvBuffer[index:]        
            else:
                self.process_receive_line(self.recvBuffer)
                self.recvBuffer = None
                break

    # NOTE(donghyun-you): line including line endings(\n)
    def process_receive_line(self,line):
        #print "Verbose> processing line: "+line
        if(self.response is None):
            self.response = self.Response()
        
        if(self.response.isReadingBody):

            if(self.response.body is None):
                self.response.body = line

            else:
                self.response.body += line

            if(len(self.response.body) >= self.response.length):
                
                body = self.response.body
                try:
                    body = json.loads(self.response.body) 
                except ValueError:
                    body = body

                self.receive_message(self.response.messageType,body)
                self.unset_processing_message()
            #done

        else:
            if(line.startswith("Length: ")):
                lengthStr=line[(len("Length: ")):].strip()
                self.response.length = int(lengthStr)
                #print "Verbose> message body length recognized: "+lengthStr

            elif(line.startswith("MessageType: ")):
                messageTypeStr=line[len("MessageType: "):].strip()
                self.response.messageType = messageTypeStr
                #print "Verbose> message type recognized: "+messageTypeStr

            elif(line == "\n"):
                if(self.response.length >= 0 and self.response.messageType is not None):
                    self.response.isReadingBody = True

                else:
                    #dropping message 
                    #print "Verbose> dropping message. invalid message received"
                    self.unset_processing_message()
                    
    def receive_message(self,message_type,body):
        #print "Verbose> receiving message"
        #print "MessageType: "+message_type
        if(message_type == "STDOUT"):
            sys.stdout.write(body)
            sys.stdout.write("\n")

        elif(message_type == "STDERR"):
            sys.stderr.write(body)
            sys.stderr.write("\n")

        else:
            print message_type
            print body

    def unset_processing_message(self):
        self.recvBuffer = None
        self.response = None
        

client = UniTestClient(HOST,PORT,"STDIN",{"func":"RunAllTest"})
asyncore.loop()
