#!/bin/bash
# wait-for-it.sh

set -e

host="$1"
port="$2"
shift 2
cmd="$@"

until [[ $(getent hosts $host) ]]; do
  echo "Waiting for $host to be resolvable..."
  sleep 2
done

ip=$(getent hosts $host | awk '{ print $1 }')
echo "Resolved $host to $ip"

until nc -z -v -w30 $ip $port; do
  echo "$host:$port is unavailable - sleeping"
  sleep 1
done

echo "$host:$port is up - executing command"
exec $cmd