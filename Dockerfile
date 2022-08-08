FROM ubuntu:bionic
ARG DEBIAN_FRONTEND=noninteractive
ARG DOCKER_VERSION=17.06.0-ce
RUN apt-get update && \
apt-get install -y libglu1 xvfb libxcursor1
COPY ./ /root/build/
WORKDIR /root/
ENTRYPOINT ["/bin/bash", "/root/build/entrypoint.sh"]