# #!/bin/bash
# # wait-for-it.sh

# set -e

# host="$1"
# shift
# cmd="$@"

# max_attempts=30
# attempts=0

# until nc -z "$host" 1433; do
#   attempts=$((attempts + 1))
#   if [ "$attempts" -ge "$max_attempts" ]; then
#     echo "SQL Server did not start in time"
#     exit 1
#   fi
#   >&2 echo "SQL Server is unavailable - sleeping"
#   sleep 1
# done

# >&2 echo "SQL Server is up - executing command"
# exec $cmd


#!/bin/bash
# wait-for-it.sh

set -e

host="$1"
port="$2"
shift 2
cmd="$@"

until [[ $(dig +short $host) ]]; do
  echo "Waiting for $host to be resolvable..."
  sleep 2
done

ip=$(dig +short $host)
echo "Resolved $host to $ip"

until nc -z -v -w30 $ip $port; do
  echo "$host:$port is unavailable - sleeping"
  sleep 1
done

echo "$host:$port is up - executing command"
exec $cmd