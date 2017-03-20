(require '[clojure.math.combinatorics :as combo])

(defn rand-int32 []
  (* (rand-int (bit-shift-left 1 31))
     (if (>= (rand) 0.5) 1 -1)))

(defn rand-distinct-reg [n]
  (-> (reduce (fn [m _]
                (loop [r (rand-int 32)]
                  (if (contains? m r)
                    (recur (rand-int 32))
                    (conj m r))))
              #{} (range n))
      vec))
(defn rand-uint32 []
  (* (long (rand-int (bit-shift-left 1 31)))
     (if (>= (rand) 0.5) 2 1)))

(defn rand-uint16 []
  (rand-int (bit-shift-left 1 16)))

; Constants
(def MAX-INT32 (dec (bit-shift-left 1 31)))
(def MIN-INT32 (- (bit-shift-left 1 31)))
(def MAX-UINT32 (dec (bit-shift-left 1 32)))
(def MIN-UINT32 0)
(def MAX-INT16 (dec (bit-shift-left 1 15)))
(def MIN-INT16 (- (bit-shift-left 1 15)))
(def MAX-UINT16 (dec (bit-shift-left 1 16)))
(def MIN-UINT16 0)

(defn to-uint32
  "I am quite sure this is a hack ... reminding me why high level
  languages are not cool for bit programming" 
  [x]
  (bit-and (long x) 0x00000000FFFFFFFF))

(defn rand-reg []
  (inc (rand-int 25)))

(defn format-uint32 [x]
  (cond (string? x)
        x
        (number? x)
        (to-uint32 x)))

(defn make-sign-extender
  [n]
  (fn [x]
    (let [sign-bit (bit-and 0x1 (bit-shift-right x (dec n)))]
      (if (= sign-bit 1)
        (- (bit-and 0xFFFF (inc (bit-not x))))
        x))))

(defn make-zero-extender
  [n]
  (fn [x]
    (let [mask (dec (bit-shift-left 1 n))]
      (bit-and x mask))))

(def sign-extend-16 (make-sign-extender 16))
(def zero-extend-16 (make-zero-extender 16))

(defn make-immediate-arimethic-fnc
  [f]
  (fn [r-val konst]
    (f r-val (sign-extend-16 konst))))

(defn make-immediate-bitwise-fnc
  [f]
  (fn [r-val konst]
    (f r-val (zero-extend-16 konst))))

(defn all-pairs [v1 v2]
  (mapcat (fn [e]
            (mapv #(vector e %) v2))
          v1))

(defn overflow-or-zero [x]
  (if (= x "OVERFLOW")
    x 0))

(defn gen-arithmetic-with-zeroes
  "n is the number of zero registers. n can be a value from 0 to 2 inclusive" 
  [n {:keys [prng fnc edge-cases formatter] :or {formatter identity}}]
  (with-out-str
    (doseq [indices (combo/combinations [0 1] n)]
      (let [zero-reg?
              (fn [i]
                (some (partial = i) (vec indices)))
            our-rand-reg
              (fn [i]
                (if (zero-reg? i) 0 (rand-reg)))
            a (prng)
            b (rand-uint16)
            rs (our-rand-reg 0)
            rt (our-rand-reg 1)
            res (cond (= rt 0)
                      (overflow-or-zero
                        (fnc (if (zero-reg? 0) 0 a) b))
                      ; run fnc with the values in register
                      ; need to check if the register is 0 or 1
                      :else
                      (fnc (if (zero-reg? 0) 0 a)
                           b))]
        (println rs (formatter a)
                 rt
                 (formatter b) (formatter res))))))

(defn gen-arithmetic-edge-cases
  [{:keys [prng fnc formatter reg-edge-cases konst-edge-cases] :or {formatter identity}}]
  (with-out-str
    (doseq [[x y] (all-pairs reg-edge-cases konst-edge-cases)]
      (println 1 (formatter x)
               2
               (formatter y) (formatter (fnc x y)))
      (println 0 (formatter x)
               2
               (formatter y) (formatter (fnc 0 y)))
      (println 1 (formatter x)
               0
               (formatter y) (overflow-or-zero (fnc x y))))))

(defn signed-add [a b]
  (let [res (+ a b)]
    (if (or (and (< a 0) (< b 0)
                 (< res -0x80000000))
            (and (> a 0) (> b 0)
                 (>= res 0x80000000)))
        "OVERFLOW"
        res)))

(defn abs [a]
  (if (> a 0) a (- a)))

(defn signed-sub [a b]
  (signed-add a (- b)))

(defn checked-div-hi [a b]
  (if (= b 0)
    "UNDEFINED"
    ; force the remainder to a positive value
    (let [remainder (mod (+ (abs b) (mod a b)) (abs b))]
      (if (< a 0)
        (- remainder (abs b))
        remainder))))

(defn checked-div-lo [a b]
  (if (= b 0)
    "UNDEFINED"
    (int (/ a b))))

(defn gen-arithmetic-data [{:keys [prng fnc] :as args}]
  (assert (and prng fnc))
  (with-out-str
    (dotimes [i 3]
      (dotimes [j 7]
        (print (gen-arithmetic-with-zeroes
                 i args))))
    (print (gen-arithmetic-edge-cases args))))

(comment (print (gen-arithmetic-edge-cases
             edge-cases args)))

["AND"  bit-and]
["OR"   bit-or]
["XOR"  bit-xor]
["ADDU" +]
["SUBU" -]
["SLTU" (fn [a b] (if (< a b) 1 0))]
["SUB" signed-sub]
["SLLV" (fn [a b] (bit-shift-left b (bit-and a 0x1F)))]
["SRAV" (fn [a b] (bit-shift-right b (bit-and a 0x1F)))]
["SRLV" (fn [a b] (bit-shift-right b (bit-and a 0x1F)))]
["SRL" (fn [a b] (bit-shift-right a (bit-and b 0x1F)))]
["SRA" (fn [a b] (bit-shift-right a (bit-and b 0x1F)))]


["ADDIU" +]
["ANDI" bit-and]
["ORI" bit-or]
["XORI" bit-xor]
["SLTI" (make-immediate-arimethic-fnc (fn [a b] (if (< a b) 1 0)))]
["SLTIU" (make-immediate-arimethic-fnc (fn [a b] (if (< a b) 1 0)))]

["ADDIU" (make-immediate-arimethic-fnc signed-add)]

["ANDI" (make-immediate-bitwise-fnc bit-and)]
["ORI" (make-immediate-bitwise-fnc bit-or)]
["XORI" (make-immediate-bitwise-fnc bit-xor)]

["SLTIU" (make-immediate-arimethic-fnc (fn [a b] (if (< (to-uint32 a)
                                            (to-uint32 b))
                                       1 0)))]

(defn run []
  (doseq [[instruction fnc] [

["SLTI" (make-immediate-arimethic-fnc (fn [a b] (if (< a b) 1 0)))]

                             ]]
    (spit (str instruction ".txt")
            (gen-arithmetic-data {:prng rand-int32 
                                  :fnc fnc
                                  :reg-edge-cases [MIN-INT32 MAX-INT32 0 1 -1]
                                  :konst-edge-cases [MIN-INT16 MAX-INT16 MIN-UINT16 MAX-UINT16 1]}))) 
  )

(comment
  (doseq [[instruction fnc] [
                             ]]
    (spit (str instruction ".txt")
            (gen-arithmetic-data {:prng rand-uint32 
                                  :formatter to-uint32
                                  :fnc fnc
                                  :reg-edge-cases [MIN-UINT32 MAX-UINT32 0 1 -1]
                                  :konst-edge-cases [MIN-INT16 MAX-INT16 MIN-UINT16 MAX-UINT16 1]})))
  ; signed stuff
  (doseq [[instruction fnc] [


                             ]]
    (spit (str instruction ".txt")
            (gen-arithmetic-data {:prng rand-int32 
                                  :fnc fnc
                                  :reg-edge-cases [MIN-INT32 MAX-INT32 0 1 -1]
                                  :konst-edge-cases [MIN-INT16 MAX-INT16 MIN-UINT16 MAX-UINT16 1]}))) 
  ; unsigned stuff
  (doseq [[instruction fnc] []]
    (spit (str instruction ".txt")
            (gen-arithmetic-data {:prng rand-uint32 
                                  :fnc fnc
                                  :formatter to-uint32
                                  :edge-cases [MIN-UINT32 MAX-UINT32 0 1]})))
  ; Signed stuff
  (doseq [[instruction fnc] []]
    (spit (str instruction ".txt")
            (gen-arithmetic-data {:prng rand-int32
                                  :fnc fnc
                                  :edge-cases [MIN-INT32 MIN-INT32 0 -1 1]}))))
  
